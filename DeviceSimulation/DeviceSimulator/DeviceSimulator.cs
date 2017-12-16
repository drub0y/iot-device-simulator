using Bogus;
using DeviceSimulation.Common.Models;
using DeviceSimulator.Extensions;
using DeviceSimulator.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Message = Microsoft.Azure.Devices.Client.Message;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace DeviceSimulator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DeviceSimulator
        : StatelessService
    {
        private readonly DeviceClient deviceClient;
        private readonly RegistryManager registryManager;
        private readonly string deviceName;
        private readonly string deviceType;

        public DeviceSimulator(StatelessServiceContext context)
            : base(context)
        {
            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var connectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["IoTHubConnectionString"];
            var connectionString = connectionStringParameter.Value;

            var bytes = Context.InitializationData;
            var json = Encoding.ASCII.GetString(bytes);
            var simulationItem = JsonConvert.DeserializeObject<SimulationItem>(json);

            deviceName = simulationItem.DeviceName;
            deviceType = simulationItem.DeviceType;

            var deviceConnectionString = $"{connectionString};DeviceId={deviceName}";

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
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
            ServiceEventSource.Current.ServiceMessage(Context, $"Reading configuration file");

            await deviceClient.OpenAsync();

            ServiceEventSource.Current.ServiceMessage(Context, $"Using device name {deviceName} and device type {deviceType}");

            var device = await registryManager.GetDeviceAsync(deviceName);
            if (device == null)
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceName));
            }

            // TODO: these definitions need to come from a configuraiton file...
            var randomizer = new Randomizer();

            var truckData = new Faker<TruckData>()
                .RuleFor(td => td.DeviceId, device.Id)
                .RuleFor(td => td.DeviceType, deviceType)
                .RuleFor(td => td.Latitude, f => f.Address.Latitude())
                .RuleFor(td => td.Longitude, f => f.Address.Longitude())
                .Generate();

            var twin = await deviceClient.GetTwinAsync();
            if (twin == null)
            {
                twin = new Twin(device.Id);
            }

            twin.Tags["IsSimulated"] = "Y";
            twin.Properties.Desired["Latitude"] = truckData.Latitude;
            twin.Properties.Reported["Latitude"] = truckData.Latitude;
            twin.Properties.Desired["Longitude"] = truckData.Longitude;
            twin.Properties.Reported["Longitude"] = truckData.Longitude;

            await registryManager.UpdateTwinAsync(device.Id, twin, "*");

            ServiceEventSource.Current.ServiceMessage(Context, $"Sending data for {truckData.DeviceId}");
            while (true)
            {
                truckData.Latitude.TweakValue(5);
                truckData.Longitude.TweakValue(5);

                var messageJson = JsonConvert.SerializeObject(truckData);
                var encodedMessage = Encoding.ASCII.GetBytes(messageJson);
                await deviceClient.SendEventAsync(new Message(encodedMessage));

                ServiceEventSource.Current.ServiceMessage(Context, $"Sending message for {truckData.DeviceId}");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
