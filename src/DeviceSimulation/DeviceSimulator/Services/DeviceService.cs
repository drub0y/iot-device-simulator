using DeviceSimulator.Interfaces;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Fabric;
using System.Text;
using System.Threading.Tasks;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace DeviceSimulator.Services
{
    public class DeviceService
        : IDeviceService
    {
        private readonly StatelessServiceContext context;
        private readonly ILoggingService loggingService;

        private readonly string hubname;
        private DeviceClient deviceClient;
        private readonly RegistryManager registryManager;

        private readonly string deviceName;
        private readonly string deviceType;

        public DeviceService(StatelessServiceContext context, ILoggingService loggingService, string connectionString, string hubname, string deviceName, string deviceType)
        {
            this.context = context;
            this.loggingService = loggingService;

            this.hubname = hubname;
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            this.deviceName = deviceName;
            this.deviceType = deviceType;
        }

        public async Task ConnectAsync()
        {
            loggingService.LogInfo($"Using device name {deviceName} and device type {deviceType}");

            var device = await registryManager.GetDeviceAsync(deviceName);
            if (device == null)
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceName));
            }

            var deviceKeyInfo = new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, device.Authentication.SymmetricKey.PrimaryKey);
            deviceClient = DeviceClient.Create($"{hubname}.azure-devices.net", deviceKeyInfo);
            await deviceClient.OpenAsync();
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
