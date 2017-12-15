using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceGenerator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DeviceGenerator
        : StatelessService
    {
        private readonly Dictionary<string, ApplicationDescription> applicationDescriptions;
        private readonly FabricClient fabricClient;

        public DeviceGenerator(StatelessServiceContext context)
            : base(context)
        {
            applicationDescriptions = new Dictionary<string, ApplicationDescription>()
            {
                {
                    "Truck",
                    new ApplicationDescription(new Uri($"fabric:/DeviceSimulation/SimulatedTruck-001"), "DeviceSimulationType", "1.0.0", new NameValueCollection()
                    {
                        { "Application_DeviceName", "Truck-001" },
                        { "Application_DeviceType", "Truck" }
                    })
                    {
                        MaximumNodes = 1
                    }
                }
            };

            fabricClient = new FabricClient();
        }

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

            foreach (var kvp in applicationDescriptions)
            {
                await fabricClient.ApplicationManager.CreateApplicationAsync(kvp.Value);
            }
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            foreach (var kvp in applicationDescriptions)
            {
                var applicationUri = kvp.Value.ApplicationName;
                var deleteApplicationDescription = new DeleteApplicationDescription(applicationUri);
                await fabricClient.ApplicationManager.DeleteApplicationAsync(deleteApplicationDescription);
            }

            await base.OnCloseAsync(cancellationToken);
        }
    }
}
