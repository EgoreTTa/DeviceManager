namespace DeviceManagerService.Configurations.Device.Connection
{
    public struct NetworkConnection
    {
        public NetworkModes Mode { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
    }
}