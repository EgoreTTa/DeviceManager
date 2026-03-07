namespace DeviceManagerService.Configurations.Device.Connection
{
    using Newtonsoft.Json.Converters;
    using System.IO.Ports;
    using Newtonsoft.Json;

    public struct SerialConnection
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Parity Parity { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StopBits StopBits { get; set; }
    }
}