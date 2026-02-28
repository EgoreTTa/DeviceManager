namespace BaseDriver
{
    using System;
    using System.IO.Ports;
    using System.Net;
    using System.Text;

    public class BaseDriver : IDriver
    {
        private BaseSession _session;
        private ConnectionTypes? _connectionType;
        private TcpIpConnection? _tcpIpConnection;
        private SerialConnection? _serialConnection;
        private FileSystemConnection? _fileSystemConnection;
        private DeviceRegistry _deviceRegistry;

        public void SetDevice(string name, string systemName) { }
        public void SetDriver(string name, string systemName, Encoding encoding) { }

        public void SetConnectionType(int type)
        {
            switch (type)
            {
                case 0:
                    _connectionType = ConnectionTypes.TcpIp;
                    break;
                case 1:
                    _connectionType = ConnectionTypes.Serial;
                    break;
                case 2:
                    _connectionType = ConnectionTypes.FileSystem;
                    break;
                default:
                    Console.WriteLine($"{DateTime.Now}\t" + $"{nameof(BaseDriver)}.{nameof(BaseDriver)}.{nameof(SetConnectionType)}");
                break;
            }
        }

        public void SetConnectionSettings(string portName, int baudRate, int parity, int dataBits, int stopBits)
        {
            _serialConnection = new SerialConnection()
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = (Parity)parity,
                DataBits = dataBits,
                StopBits = (StopBits)stopBits,
            };
        }

        public void SetConnectionSettings(string mode, IPEndPoint endPoint)
        {
            _tcpIpConnection = new TcpIpConnection()
            {
                Mode = mode == "Server" ? TcpIpModes.Server : TcpIpModes.Client,
                IpEndPoint = endPoint
            };
        }

        public void SetConnectionSettings(string folderToRead, string folderToWrite)
        {
            _fileSystemConnection = new FileSystemConnection()
            {
                FolderToRead = folderToRead,
                FolderToWrite = folderToWrite
            };
        }

        public void SetRegistry(DeviceRegistry deviceRegistry)
        {
            _deviceRegistry = deviceRegistry;
            _connectionType = deviceRegistry.ConnectionType;
            _tcpIpConnection = new TcpIpConnection()
            {
                Mode = deviceRegistry.TcpIp.Mode,
                IpEndPoint = deviceRegistry.TcpIp.IpEndPoint
            };
            _serialConnection = new SerialConnection()
            {
                PortName = deviceRegistry.Serial.PortName,
                BaudRate = deviceRegistry.Serial.BaudRate,
                Parity = deviceRegistry.Serial.Parity,
                DataBits = deviceRegistry.Serial.DataBits,
                StopBits = deviceRegistry.Serial.StopBits,
            };
            _fileSystemConnection = new FileSystemConnection()
            {
                FolderToRead = deviceRegistry.FileSystem.FolderToRead,
                FolderToWrite = deviceRegistry.FileSystem.FolderToWrite
            };
        }

        public void Start()
        {
            _session = new BaseSession(_deviceRegistry);
            _session.Start();
        }

        public void Stop()
        {
            _session.Stop();
        }
    }
}