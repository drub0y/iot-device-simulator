using CodeEngine.CSharp;
using CodeEngine.CSharp.Interfaces;
using CodeEngine.FSharp;
using CodeEngine.FSharp.Interfaces;
using CodeEngine.Interfaces;
using CodeEngine.JavaScript;
using CodeEngine.JavaScript.Interfaces;
using CodeEngine.Python;
using CodeEngine.Python.Interfaces;
using CodeEngine.Services;
using DeviceSimulation.Common.Models;
using DeviceSimulator.Interfaces;
using DeviceSimulator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DeviceSimulator
        : StatelessService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly SimulationItem simulationItem;

        private readonly IDeviceStore deviceStore;
        private readonly ILoggingService loggingService;
        private readonly ICSharpService<string> csharpService;

        public DeviceSimulator(StatelessServiceContext context)
            : base(context)
        {
            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var iotHubConnectionStringParameter = configurationPackage.Settings.Sections["ConnectionStrings"].Parameters["IoTHubConnectionString"];
            var iotHubConnectionString = iotHubConnectionStringParameter.Value;

            var hubNameParameter = configurationPackage.Settings.Sections["IoTHub"].Parameters["HubName"];
            var hubName = hubNameParameter.Value;

            var bytes = Context.InitializationData;
            var json = Encoding.ASCII.GetString(bytes);
            simulationItem = JsonConvert.DeserializeObject<SimulationItem>(json);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IFileService, FileService>();
            serviceCollection.AddScoped<ICSharpService<string>, CSharpService<string>>();
            serviceCollection.AddScoped<IFSharpService<string>, FSharpService<string>>();
            serviceCollection.AddScoped<IPythonService<string>, PythonService<string>>();
            serviceCollection.AddScoped<IJavaScriptService<string>, JavaScriptService<string>>();
            serviceCollection.AddScoped<ICodeService<FileInfo, string>, CodeService<FileInfo, string>>();
            serviceCollection.AddScoped<ILoggingService, LoggingService>((sp) =>
            {
                return new LoggingService(context);
            });

            serviceCollection.AddScoped<IDeviceStore, DeviceStore>((sp) =>
            {
                var deviceStore = new DeviceStore();
                for (int i = simulationItem.DeviceStartRange; i <= simulationItem.DeviceEndRange; i++)
                {
                    var loggingService = sp.GetRequiredService<ILoggingService>();
                    var deviceName = simulationItem.DevicePrefix + '-' + i;
                    var device = new DeviceService(context, loggingService, iotHubConnectionString, hubName, deviceName, simulationItem.DeviceType);
                    deviceStore.Devices().Add(device);
                }
                return deviceStore;
            });

            serviceProvider = serviceCollection.BuildServiceProvider();

            // TODO: Move to Program.cs
            loggingService = serviceProvider.GetRequiredService<ILoggingService>();
            csharpService = serviceProvider.GetRequiredService<ICSharpService<string>>();
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

            // TODO: these definitions need to come from a configuraiton file...
            //#if DEBUG
            //            Thread.Sleep(10000);
            //            Debugger.Break();
            //#endif

            var interval = simulationItem.Interval * 1000;
            dynamic initialState = JObject.Parse(simulationItem.InitialState);
            var deviceState = new Dictionary<string, dynamic>();

            foreach (var device in deviceStore.Devices())
            {
                try
                {
                    await device.ConnectAsync();
                    deviceState.Add(device.DeviceName, deviceState);
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    loggingService.LogInfo("Error: " + ex.Message);
                    throw ex;
                }
            }

            while (true)
            {
                foreach (var device in deviceStore.Devices())
                {
                    string json = JsonConvert.SerializeObject(deviceState[device.DeviceName]);

                    var currentState = await csharpService.ExecuteAsync(simulationItem.ScriptFile, json);
                    await device.SendEventAsync(currentState, simulationItem.MessageType);

                    deviceState[device.DeviceName] = JObject.Parse(currentState);
                }

                await Task.Delay(TimeSpan.FromSeconds(simulationItem.Interval), cancellationToken);
            }
        }
    }
}
