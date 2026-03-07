namespace DeviceManagerService.Configurations
{
    public struct DeviceManagerConfiguration
    {
        public string LisUrl { get; set; }
        public int TimeOutInSecond { get; set; }
        public bool LogRequests { get; set; }
    }
}