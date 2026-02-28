namespace API.Controllers.Devices
{
    public class FormHardware
    {
        public string Type { get; set; } = string.Empty;
        public TcpIpConnection TcpIp { get; set; } = new TcpIpConnection() { };
        public SerialConnection Serial { get; set; } = new SerialConnection() { };
        public FileSystemConnection FileSystemSystem { get; set; } = new FileSystemConnection() { };

        public class TcpIpConnection
        {
            public string Mode { get; set; } = string.Empty;
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; }
        }

        public class SerialConnection
        {
            public string PortName { get; set; } = string.Empty;
            public string BaudRate { get; set; } = string.Empty;
            public string DataBits { get; set; } = string.Empty;
            public string StopBits { get; set; } = string.Empty;
            public string Parity { get; set; } = string.Empty;
        }

        public class FileSystemConnection
        {
            public string FolderToRead { get; set; } = string.Empty;
            public string ReadPeriodInSeconds { get; set; } = string.Empty;
            public string FolderToWrite { get; set; } = string.Empty;
            public string FileToWriteNameMask { get; set; } = string.Empty;
            public string DeleteAfterRead { get; set; } = string.Empty;
        }
    }
}