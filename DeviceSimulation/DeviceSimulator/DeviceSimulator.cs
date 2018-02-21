using Bogus;
using DeviceSimulation.Common.Models;
using DeviceSimulator.Interfaces;
using DeviceSimulator.Models;
using DeviceSimulator.Services;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System;
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
        private readonly SimulationItem simulationItem;
        private readonly IDeviceService deviceService;

        public DeviceSimulator(StatelessServiceContext context)
            : base(context)
        {
            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var iotHubConnectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["IoTHubConnectionString"];
            var iotHubConnectionString = iotHubConnectionStringParameter.Value;

            var storageAccouuntConnectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["StorageAccountConnectionString"];
            var storageAccountConnectionString = storageAccouuntConnectionStringParameter.Value;

            var hubNameParameter = configurationPackage.Settings.Sections["IoTHub"].Parameters["HubName"];
            var hubName = hubNameParameter.Value;

            var bytes = Context.InitializationData;
            var json = Encoding.ASCII.GetString(bytes);
            simulationItem = JsonConvert.DeserializeObject<SimulationItem>(json);

            var storageService = new StorageService(context, storageAccountConnectionString);
            deviceService = new DeviceService(context, iotHubConnectionString, hubName, simulationItem.DeviceName, simulationItem.DeviceType);
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

            // TODO: these definitions need to come from a configuraiton file...
            var randomizer = new Randomizer();

            TruckData truckData =new TruckData();
            try
            {
                truckData = new Faker<TruckData>()
                    .RuleFor(td => td.DeviceId, simulationItem?.DeviceName ?? "device-unkown")
                    .RuleFor(td => td.DeviceType, simulationItem?.DeviceType ?? "type-unkown")
                    .RuleFor(td => td.Latitude, f => f.Address.Latitude())
                    .RuleFor(td => td.Longitude, f => f.Address.Longitude())
                    .Generate();
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(Context, $"{ex.ToString()}");
            }

            //TODO: Should handle disconnects while sending data in loop
            await deviceService.ConnectAsync();

            ServiceEventSource.Current.ServiceMessage(Context, $"Sending data for {truckData.DeviceId}");
            while (true)
            {
                // Would want better values here...
                truckData.Latitude = randomizer.Double(55, 80);
                truckData.Longitude = randomizer.Double(60, 90);

                var messageJson = JsonConvert.SerializeObject(truckData);
                var encodedMessage = Encoding.ASCII.GetBytes(messageJson);
                await deviceService.SendEventAsync(truckData);

                ServiceEventSource.Current.ServiceMessage(Context, $"Sending message for {truckData.DeviceId}");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }

    public class TruckData
    {
        public string DeviceId { get; set; }

        public string DeviceType { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
