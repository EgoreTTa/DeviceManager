namespace DeviceManagerAPI.Controllers.Devices.Forms
{
    public class FormHardware
    {
        public ConnectionTypes ConnectionType { get; set; }
        public NetworkConnection Network { get; set; } = new NetworkConnection() { };
        public SerialConnection Serial { get; set; } = new SerialConnection() { };
        public FileSystemConnection FileSystem { get; set; } = new FileSystemConnection() { };

        public enum ConnectionTypes
        {
            Network,
            Serial,
            FileSystem,
        }

        public class NetworkConnection
        {
            public Modes Mode { get; set; }
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; }

            public enum Modes
            {
                Server,
                Client,
            }
        }

        public class SerialConnection
        {
            public string PortName { get; set; } = string.Empty;
            public int BaudRate { get; set; }
            public int DataBits { get; set; }
            public StopBits StopBit { get; set; }
            public Parities Parity { get; set; }

            public enum StopBits
            {
                None,
                One,
                Two,
                OnePointFive,
            }
            public enum Parities
            {
                None,
                Odd,
                Even,
                Mark,
                Space,
            }
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