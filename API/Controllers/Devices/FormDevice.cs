namespace API.Controllers.Devices
{
    public class FormDevice
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public FormDeviceDriver Driver { get; set; } = new FormDeviceDriver() { };
        public FormHardware Hardware { get; set; } = new FormHardware() { };
        public bool IsActive { get; set; } = true;
    }
}