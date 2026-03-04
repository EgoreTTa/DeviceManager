namespace DeviceManagerService.Configurations.Device.Connection
{
    public struct ConnectionConfiguration
    {
        public ConnectionTypes ConnectionType { get; set; }
        public NetworkConnection Network { get; set; }
        public SerialConnection Serial { get; set; }
        public FileSystemConnection FileSystem { get; set; }
    }
}