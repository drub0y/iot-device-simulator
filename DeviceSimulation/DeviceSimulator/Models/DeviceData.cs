using Newtonsoft.Json;
using System;

namespace DeviceSimulator.Models
{
    public abstract class DeviceData
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("deviceType")]
        public string DeviceType { get; set; }

        [JsonProperty("sampleDateTime")]
        public DateTime SampleDateTime { get; set; }
    }
}
