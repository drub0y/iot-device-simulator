using System.Threading.Tasks;

namespace DeviceSimulator.Interfaces
{
    public interface IDeviceService
    {
        Task ConnectAsync();
        Task SendEventAsync<T>(T item, string messageType);
        string DeviceName { get; }
    }
}
