namespace DeviceManager.Configurations.Device.Driver
{
    using DriverBase.DTOs;

    public class DriverInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public ParserInfo[] Parsers { get; set; }
    }

    public class ParserInfo
    {
        public string FullName { get; set; }
        public OptionDTO[] Options { get; set; }
    }
}