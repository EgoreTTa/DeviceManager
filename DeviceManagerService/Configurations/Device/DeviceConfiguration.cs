namespace DeviceManagerService.Configurations.Device
{
    using Connection;
    using Driver;

    public struct DeviceConfiguration
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string DeviceSystemName { get; set; }
        public DriverConfiguration DriverConfiguration { get; set; }
        public ConnectionConfiguration ConnectionConfiguration { get; set; }
    }
}