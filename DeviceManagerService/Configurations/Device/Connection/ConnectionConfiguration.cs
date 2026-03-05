namespace DeviceManagerService.Configurations.Device.Connection
{
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json;

    public struct ConnectionConfiguration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ConnectionTypes ConnectionType { get; set; }
        public NetworkConnection Network { get; set; }
        public SerialConnection Serial { get; set; }
        public FileSystemConnection FileSystem { get; set; }
    }
}