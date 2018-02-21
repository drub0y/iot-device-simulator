using DeviceSimulation.Common.Models;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceGenerator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DeviceGenerator
        : StatelessService
    {
        private readonly Uri applicationPath;
        private readonly Dictionary<string, StatelessServiceDescription> serviceDescriptions;
        private readonly FabricClient fabricClient;

        public DeviceGenerator(StatelessServiceContext context)
            : base(context)
        {
            applicationPath = new Uri($"fabric:/DeviceSimulation/Devices");

            var simulations = new List<SimulationItem>();
            var simulationIds = Enumerable.Range(1, 1);
            foreach (var simulationId in simulationIds)
            {
                var simulationItem = new SimulationItem()
                {
                    Id = Guid.NewGuid(),
                    DeviceName = $"SimulatedTruck-{simulationId.ToString("0000")}",
                    DeviceType = "Truck",
                    DefinitionPath = "Truck.json",
                    Interval = 1
                };
                simulations.Add(simulationItem);
            }

            serviceDescriptions = new Dictionary<string, StatelessServiceDescription>();
            foreach (var simulation in simulations)
            {
                var json = JsonConvert.SerializeObject(simulation);
                var statelessServiceDescription = new StatelessServiceDescription()
                {
                    ApplicationName = new Uri($"fabric:/DeviceSimulation/Devices"),
                    ServiceName = new Uri($"fabric:/DeviceSimulation/Devices/{simulation.DeviceName}"),
                    ServiceTypeName = "DeviceSimulatorType",
                    PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                    InitializationData = Encoding.ASCII.GetBytes(json),
                    InstanceCount = 1,
                };

                serviceDescriptions.Add(simulation.DeviceName, statelessServiceDescription);
            }

            fabricClient = new FabricClient();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var applicationDescription = new ApplicationDescription(applicationPath, "DeviceSimulationType", "1.0.0", new NameValueCollection());
            await fabricClient.ApplicationManager.CreateApplicationAsync(applicationDescription);
            foreach (var kvp in serviceDescriptions)
            {
                await fabricClient.ServiceManager.CreateServiceAsync(kvp.Value);
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
