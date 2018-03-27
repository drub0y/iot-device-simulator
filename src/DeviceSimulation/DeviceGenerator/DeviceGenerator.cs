using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Fabric;
using System.Fabric.Description;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceGenerator.Interfaces;
using DeviceGenerator.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using DeviceSimulation.Common.Models;

namespace DeviceGenerator
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class DeviceGenerator : StatelessService
    {

        private readonly Uri applicationPath;
        private readonly IStorageService storageService;

        private Dictionary<string, StatelessServiceDescription> serviceDescriptions;
        private FabricClient fabricClient;

        public DeviceGenerator(StatelessServiceContext context)
            : base(context)
        {

            applicationPath = new Uri($"fabric:/DeviceSimulation/Devices");

            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var storageAccouuntConnectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["StorageAccountConnectionString"];
            var storageAccountConnectionString = storageAccouuntConnectionStringParameter.Value;
            storageService = new StorageService(storageAccountConnectionString);

        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatelessServiceContext>(serviceContext))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var simulationJson = await storageService.FetchFileAsync("run", "main-simulation.json");
            var simulations = JsonConvert.DeserializeObject<IEnumerable<SimulationItem>>(simulationJson);

            var ramp = 1;
            var rampDelay = 1;

            serviceDescriptions = new Dictionary<string, StatelessServiceDescription>();
            foreach (var simulation in simulations)
            {
                simulation.ScriptFile = await storageService.FetchFileAsync("scripts", $"{simulation.DeviceType}.cscript");
                simulation.ScriptLanguage = ScriptLanguage.CSharp;
                simulation.InitialState = await storageService.FetchFileAsync("state", $"{simulation.DeviceType}.json");
                var batchStart = 1;
                var batchSize = 10;

                batchSize = simulation.BatchSize ?? batchSize;
                var batches = Enumerable.Range(simulation.DeviceOffset ?? 1, (simulation.NumberOfDevices + (simulation.DeviceOffset ?? 1)) / batchSize);

                var serviceName = $"{simulation.DeviceStartRange}-{simulation.DeviceEndRange}";
                var json = JsonConvert.SerializeObject(simulation);
                var statelessServiceDescription = new StatelessServiceDescription()
                {
                    ApplicationName = new Uri($"fabric:/DeviceSimulation/Devices"),
                    ServiceName = new Uri($"fabric:/DeviceSimulation/Devices/{serviceName}"),
                    ServiceTypeName = "DeviceSimulatorType",
                    PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                    InitializationData = Encoding.ASCII.GetBytes(json),
                    InstanceCount = 1,
                };

                serviceDescriptions.Add(serviceName, statelessServiceDescription);
                batchStart += batchSize;
            }

            fabricClient = new FabricClient();
            var applicationDescription = new ApplicationDescription(applicationPath, "DeviceSimulationType", "1.0.0", new NameValueCollection());
            await fabricClient.ApplicationManager.CreateApplicationAsync(applicationDescription);

            int i = 1;
            foreach (var kvp in serviceDescriptions)
            {
                await fabricClient.ServiceManager.CreateServiceAsync(kvp.Value);

                // Stagger Service Start Times
                if (i % ramp == 0 || i == serviceDescriptions.Count())
                {
                    System.Diagnostics.Trace.WriteLine($"Waiting {rampDelay * 1000} s to start up next batch of applications.");
                    Thread.Sleep(rampDelay * 1000);
                }

                i++;
            }
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            var deleteApplicationDescription = new DeleteApplicationDescription(applicationPath);
            await fabricClient.ApplicationManager.DeleteApplicationAsync(deleteApplicationDescription);

            await base.OnCloseAsync(cancellationToken);
        }
    }
}
