using System;

namespace DeviceSimulation.Common.Models
{
    public class SimulationItem
    {
        /// <summary>
        /// A unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the instance of this device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// The device type like Truck
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// The location of the json file
        /// </summary>
        public string DefinitionPath { get; set; }

        /// <summary>
        /// This is the time in seconds
        /// </summary>
        public int Interval { get; set; }
    }
}
