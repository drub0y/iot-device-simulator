using Newtonsoft.Json;

namespace DeviceSimulator.Models
{
    public class TruckData
        : DeviceData
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}
