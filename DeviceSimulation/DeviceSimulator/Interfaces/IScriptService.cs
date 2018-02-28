using DeviceSimulation.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulator.Interfaces
{
    public interface IScriptService
    {
        Task RunScriptAsync(SimulationItem simulationItem, CancellationToken cancellationToken);
    }
}
