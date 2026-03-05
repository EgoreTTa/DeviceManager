namespace DeviceManagerService.Configurations.Device.Driver
{
    public struct DriverConfiguration
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Encoding { get; set; }
        public string SelectParser { get; set; }
        public string[] Parsers { get; set; }
    }
}