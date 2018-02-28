using DeviceSimulator.Interfaces;
using System.Fabric;

namespace DeviceSimulator.Services
{
    public class LoggingService
        : ILoggingService
    {
        private readonly StatelessServiceContext statelessServiceContext;

        public LoggingService(StatelessServiceContext statelessServiceContext)
        {
            this.statelessServiceContext = statelessServiceContext;
        }

        public void LogInfo(string message)
        {
            ServiceEventSource.Current.ServiceMessage(statelessServiceContext, message);
        }
    }
}
