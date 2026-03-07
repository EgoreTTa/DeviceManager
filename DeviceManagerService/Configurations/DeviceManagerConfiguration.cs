namespace DeviceManager.Configurations
{
    public struct DeviceManagerConfiguration
    {
        public string Address { get; set; }
        public int TimeOutInSecond { get; set; }
        public bool LogRequests { get; set; }
    }
}