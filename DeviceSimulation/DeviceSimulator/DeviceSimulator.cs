using CodeEngine.CSharp;
using CodeEngine.CSharp.Interfaces;
using CodeEngine.FSharp;
using CodeEngine.FSharp.Interfaces;
using CodeEngine.Interfaces;
using CodeEngine.JavaScript;
using CodeEngine.JavaScript.Interfaces;
using CodeEngine.Python;
using CodeEngine.Python.Interfaces;
using CodeEngine.Services;
using DeviceSimulation.Common.Models;
using DeviceSimulator.Interfaces;
using DeviceSimulator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DeviceSimulator
        : StatelessService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly SimulationItem simulationItem;

        public DeviceSimulator(StatelessServiceContext context)
            : base(context)
        {
            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var iotHubConnectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["IoTHubConnectionString"];
            var iotHubConnectionString = iotHubConnectionStringParameter.Value;

            var hubNameParameter = configurationPackage.Settings.Sections["IoTHub"].Parameters["HubName"];
            var hubName = hubNameParameter.Value;

            var bytes = Context.InitializationData;
            var json = Encoding.ASCII.GetString(bytes);
            simulationItem = JsonConvert.DeserializeObject<SimulationItem>(json);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IFileService, FileService>();
            serviceCollection.AddScoped<ICSharpService<string>, CSharpService<string>>();
            serviceCollection.AddScoped<IFSharpService<string>, FSharpService<string>>();
            serviceCollection.AddScoped<IPythonService<string>, PythonService<string>>();
            serviceCollection.AddScoped<IJavaScriptService<string>, JavaScriptService<string>>();
            serviceCollection.AddScoped<ICodeService<FileInfo, string>, CodeService<FileInfo, string>>();
            serviceCollection.AddScoped<IScriptService, CSharpScriptService>();
            serviceCollection.AddScoped<ILoggingService, LoggingService>((sp) =>
            {
                return new LoggingService(context);
            });
            serviceCollection.AddScoped<IDeviceService, DeviceService>((sp) =>
            {
                var loggingService = sp.GetRequiredService<ILoggingService>();
                return new DeviceService(context, loggingService, iotHubConnectionString, hubName, simulationItem.DeviceName, simulationItem.DeviceType);
            });
            serviceProvider = serviceCollection.BuildServiceProvider();
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

            // TODO: these definitions need to come from a configuraiton file...
            var scriptEngine = serviceProvider.GetRequiredService<IScriptService>();
            await scriptEngine.RunScriptAsync(simulationItem, cancellationToken);
        }
    }
}
