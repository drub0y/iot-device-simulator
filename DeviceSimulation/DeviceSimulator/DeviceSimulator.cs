using Bogus;
using DeviceSimulator.Extensions;
using DeviceSimulator.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
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
        public DeviceSimulator(StatelessServiceContext context)
            : base(context)
        { }

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

            if (Context.CodePackageActivationContext.ApplicationName.EndsWith("DeviceSimulation")) return;

            ServiceEventSource.Current.ServiceMessage(Context, $"Reading configuration file");

            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var connectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["IoTHubConnectionString"];
            var devicePath = Context.ServiceName.AbsolutePath;

            var deviceTypeParameter = configurationPackage.Settings.Sections["Application"].Parameters["DeviceType"];
            var deviceType = deviceTypeParameter.Value;

            var deviceNameParameter = configurationPackage.Settings.Sections["Application"].Parameters["DeviceName"];
            var deviceName = deviceNameParameter.Value;

            ServiceEventSource.Current.ServiceMessage(Context, $"Using device name {deviceName} and device type {deviceType}");

            var device = await RegisterOrFetchDevice(deviceName, connectionStringParameter.Value);
            var deviceConnectionString = $"{connectionStringParameter.Value};DeviceId={device.Id}";

            // TODO: these definitions need to come from a configuraiton file...
            var randomizer = new Randomizer();
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
            await deviceClient.OpenAsync();

            var twin = await deviceClient.GetTwinAsync();

            var truckData = new Faker<TruckData>()
                .RuleFor(td => td.DeviceId, device.Id)
                .RuleFor(td => td.DeviceType, deviceType)
                .RuleFor(td => td.Latitude, f => f.Address.Latitude())
                .RuleFor(td => td.Longitude, f => f.Address.Longitude())
                .Generate();

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

        private static async Task<Device> RegisterOrFetchDevice(string deviceName, string connectionString)
        {
            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            var device = await registryManager.GetDeviceAsync(deviceName);
            if (device == null)
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceName));
            }
            return device;
        }
    }
}
