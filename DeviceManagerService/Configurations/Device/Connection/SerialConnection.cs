using System.IO.Ports;

namespace DeviceManagerService.Configurations.Device.Connection
{
    public struct SerialConnection
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Parity Parity { get; set; }
    }
}