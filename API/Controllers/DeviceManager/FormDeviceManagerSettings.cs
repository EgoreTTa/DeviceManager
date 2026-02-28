namespace API.Controllers.DeviceManager
{
    public class FormDeviceManagerSettings
    {
        public string Address { get; set; } = string.Empty;
        public string TimeOut { get; set; } = string.Empty;
        public bool IsLogRequest { get; set; } = false;
    }
}