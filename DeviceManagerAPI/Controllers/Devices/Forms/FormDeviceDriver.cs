namespace DeviceManagerAPI.Controllers.Devices.Forms
{
    public class FormDeviceDriver
    {
        public string Name { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public string Encoding { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty;
        public string[] Options { get; set; } = { };
        public string ExtraOptions { get; set; } = string.Empty;
    }
}