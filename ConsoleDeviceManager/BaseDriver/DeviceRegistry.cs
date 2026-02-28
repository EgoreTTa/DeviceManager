namespace BaseDriver
{
    public struct DeviceRegistry
    {
        public string DeviceName { get; set; }
        public string DeviceSystemName { get; set; }
        public string DriverName { get; set; }
        public string DriverSystemName { get; set; }
        public string Encoding { get; set; }
        public ConnectionTypes ConnectionType { get; set; }
        public TcpIpConnection TcpIp { get; set; }
        public SerialConnection Serial { get; set; }
        public FileSystemConnection FileSystem { get; set; }
    }
}