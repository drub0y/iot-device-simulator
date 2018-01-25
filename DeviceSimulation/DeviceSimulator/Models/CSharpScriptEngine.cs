using DeviceSimulator.Interfaces;
using System.Threading.Tasks;

namespace DeviceSimulator.Models
{
    public class CSharpScriptEngine
        : IScriptEngine
    {
        private readonly IStorageService storageService;
        private readonly IDeviceService deviceService;

        public CSharpScriptEngine(IStorageService storageService, IDeviceService deviceService)
        {
            this.storageService = storageService;
            this.deviceService = deviceService;
        }

        public async Task RunScriptAsync(string containerName, string fileName)
        {

        }
    }
}
