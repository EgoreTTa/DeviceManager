namespace DeviceManagerService.Configurations.Device
{
    using Connection;
    using Driver;

    public struct DeviceConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public DriverConfiguration DriverConfiguration { get; set; }
        public ConnectionConfiguration ConnectionConfiguration { get; set; }
        public bool IsActive { get; set; }
    }
}