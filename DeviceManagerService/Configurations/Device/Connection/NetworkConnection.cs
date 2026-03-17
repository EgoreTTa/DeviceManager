namespace DeviceManager.Configurations.Device.Connection
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class NetworkConnection
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NetworkModes Mode { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
    }
}