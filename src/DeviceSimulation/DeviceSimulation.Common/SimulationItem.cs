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
        /// The name of the instance of this device.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// The device type like Truck.
        /// </summary>
        public string DeviceType { get; set; }

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
        /// This is the time in seconds.
        /// </summary>
        public int Interval { get; set; }
    }
}
