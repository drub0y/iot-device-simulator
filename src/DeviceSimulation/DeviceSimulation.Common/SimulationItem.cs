using System;

namespace DeviceSimulation.Common.Models
{
    public class SimulationItem
    {
        /// <summary>
        /// A unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The base name prefix of the device.
        /// </summary>
        public string DevicePrefix { get; set; }

        /// <summary>
        /// The device type like Truck.
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Specifies the type of message being sent
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// The contents of the json initial state file.
        /// </summary>
        public string InitialState { get; set; }

        /// <summary>
        /// The contents of the script file.
        /// </summary>
        public string ScriptFile { get; set; }

        /// <summary>
        /// The language of the script file.
        /// </summary>
        public ScriptLanguage ScriptLanguage { get; set; }

        /// <summary>
        /// Specifies how many new services will be created / sec
        /// </summary>
        public int? Ramp { get; set; }

        /// <summary>
        /// Specifies the delay in between new ramp cycles
        /// </summary>
        public int? RampDelay { get; set; }

        /// <summary>
        /// This is the time in seconds.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// The total number of devices to simulate
        /// </summary>
        public int NumberOfDevices { get; set; }

        /// <summary>
        /// Optionally specifies the starting index of the devices
        /// </summary>
        public int? DeviceOffset { get; set; }

        /// <summary>
        /// Start range for devices managed by service
        /// </summary>
        public int DeviceStartRange { get; set; }

        /// <summary>
        /// End range for devices managed by service
        /// </summary>
		public int DeviceEndRange { get; set; }

        /// <summary>
        /// Batch size for how many devices each service will handle
        /// </summary>
		public int? BatchSize { get; set; }
    }
}
