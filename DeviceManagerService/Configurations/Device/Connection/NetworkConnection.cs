namespace DeviceManagerService.Configurations.Device.Connection
{
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json;

    public struct NetworkConnection
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NetworkModes Mode { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
    }
}