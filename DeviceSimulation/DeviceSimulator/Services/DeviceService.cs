using DeviceSimulator.Interfaces;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Fabric;
using System.Text;
using System.Threading.Tasks;
using Message = Microsoft.Azure.Devices.Client.Message;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace DeviceSimulator.Services
{
    public class DeviceService
        : IDeviceService
    {
        private readonly StatelessServiceContext context;
        private readonly DeviceClient deviceClient;
        private readonly RegistryManager registryManager;
        private readonly string deviceName;
        private readonly string deviceType;

        public DeviceService(StatelessServiceContext context, string connectionString, string deviceName, string deviceType)
        {
            var deviceConnectionString = $"{connectionString};DeviceId={deviceName}";

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

            this.deviceName = deviceName;
            this.deviceType = deviceType;
        }

        public async Task ConnectAsync()
        {
            ServiceEventSource.Current.ServiceMessage(context, $"Reading configuration file");

            await deviceClient.OpenAsync();

            ServiceEventSource.Current.ServiceMessage(context, $"Using device name {deviceName} and device type {deviceType}");

            var device = await registryManager.GetDeviceAsync(deviceName);
            if (device == null)
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceName));
            }
        }

        public async Task SendEventAsync<T>(T item)
        {
            var json = JsonConvert.SerializeObject(item);
            var bytes = Encoding.UTF8.GetBytes(json);
            var message = new Message(bytes);
            await deviceClient.SendEventAsync(message);
        }
    }
}
