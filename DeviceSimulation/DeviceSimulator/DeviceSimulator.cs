using DeviceSimulation.Common.Models;
using DeviceSimulator.Interfaces;
using DeviceSimulator.Models;
using DeviceSimulator.Services;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Fabric;
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
        private readonly IScriptEngine scriptEngine;

        public DeviceSimulator(StatelessServiceContext context)
            : base(context)
        {
            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var iotHubConnectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["IoTHubConnectionString"];
            var iotHubConnectionString = iotHubConnectionStringParameter.Value;

            var storageAccouuntConnectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["StorageAccountConnectionString"];
            var storageAccountConnectionString = storageAccouuntConnectionStringParameter.Value;

            var bytes = Context.InitializationData;
            var json = Encoding.ASCII.GetString(bytes);
            var simulationItem = JsonConvert.DeserializeObject<SimulationItem>(json);

            var storageService = new StorageService(context, storageAccountConnectionString);
            var deviceService = new DeviceService(context, iotHubConnectionString, simulationItem.DeviceName, simulationItem.DeviceType);
            scriptEngine = new CSharpScriptEngine(storageService, deviceService);
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



        }
    }
}
