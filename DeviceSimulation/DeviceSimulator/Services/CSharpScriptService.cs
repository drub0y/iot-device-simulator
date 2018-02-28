using CodeEngine.CSharp.Interfaces;
using DeviceSimulation.Common.Models;
using DeviceSimulator.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulator.Services
{
    public class CSharpScriptService
        : IScriptService
    {
        private readonly IDeviceService deviceService;
        private readonly ILoggingService loggingService;
        private readonly ICSharpService<string> csharpService;

        public CSharpScriptService(IDeviceService deviceService, ILoggingService loggingService, ICSharpService<string> csharpService)
        {
            this.deviceService = deviceService;
            this.loggingService = loggingService;
            this.csharpService = csharpService;
        }

        public async Task RunScriptAsync(SimulationItem simulationItem, CancellationToken cancellationToken)
        {
            var interval = simulationItem.Interval * 1000;
            var initialState = simulationItem.InitialState;
            var previousState = initialState;

            while (true)
            {
                var currentState = await csharpService.ExecuteAsync(simulationItem.ScriptFile, previousState);
                await deviceService.SendEventAsync(currentState);

                loggingService.LogInfo($"Sent data for {simulationItem.DeviceName} of type {simulationItem.DeviceType}");
                previousState = currentState;

                await Task.Delay(TimeSpan.FromSeconds(simulationItem.Interval), cancellationToken);
            }
        }
    }
}
