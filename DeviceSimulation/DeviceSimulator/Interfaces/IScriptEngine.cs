using System.Threading.Tasks;

namespace DeviceSimulator.Interfaces
{
    public interface IScriptEngine
    {
        Task RunScriptAsync(string containerName, string fileName);
    }
}
