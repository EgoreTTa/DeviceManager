namespace DeviceManager.Configurations.Device.Connection
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ConnectionConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ConnectionTypes ConnectionType { get; set; }
        public NetworkConnection Network { get; set; }
        public SerialConnection Serial { get; set; }
        public FileSystemConnection FileSystem { get; set; }
    }
}