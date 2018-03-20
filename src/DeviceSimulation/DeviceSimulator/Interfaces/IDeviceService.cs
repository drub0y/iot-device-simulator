using System.Threading.Tasks;

namespace DeviceSimulator.Interfaces
{
    public interface IDeviceService
    {
        Task ConnectAsync();
        Task SendEventAsync<T>(T item);
    }
}
