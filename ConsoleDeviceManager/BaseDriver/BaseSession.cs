namespace BaseDriver
{
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class BaseSession
    {
        private string _deviceName;
        private string _deviceSystemName;
        private SerialConnection _serialPort;
        private TcpIpConnection _tcpIip;
        private FileSystemConnection _fileSystemFolder;
        private ConnectionTypes _connectionType;
        private bool _isActive = true;

        private byte[] _toWrite;

        public string DeviceName => _deviceName;
        public string DeviceSystemName => _deviceSystemName;
        public SerialConnection SerialPort => _serialPort;
        public TcpIpConnection TcpIip => _tcpIip;
        public FileSystemConnection FileSystemFolder => _fileSystemFolder;
        public ConnectionTypes ConnectionType => _connectionType;
        public bool IsActive => _isActive;

        public BaseSession(DeviceRegistry deviceRegistry)
        {
            _deviceName = deviceRegistry.DeviceName;
            _deviceSystemName = deviceRegistry.DeviceSystemName;
            _connectionType = deviceRegistry.ConnectionType;
            _tcpIip = deviceRegistry.TcpIp;
            _serialPort = deviceRegistry.Serial;
            _fileSystemFolder = deviceRegistry.FileSystem;
        }

        public void Start()
        {
            switch (_connectionType)
            {
                case ConnectionTypes.TcpIp:
                    InitTcpIpPort(_tcpIip);
                    break;
                case ConnectionTypes.Serial:
                    InitSerialPort(_serialPort);
                    break;
                case ConnectionTypes.FileSystem:
                    InitFileSystemFolder(_fileSystemFolder);
                    break;
                default:
                    Console.WriteLine($"{DateTime.Now}\t" + $"{nameof(BaseDriver)}.{nameof(BaseSession)}.{nameof(Start)}");
                    break;
            }
        }

        public void Stop()
        {
            _isActive = false;
        }

        private void InitSerialPort(SerialConnection serial)
        {
            var serialPort = new SerialPort(serial.PortName,
                serial.BaudRate,
                serial.Parity,
                serial.DataBits,
                serial.StopBits);
            try
            {
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;
                serialPort.Open();

                try
                {
                    using var stream = serialPort.BaseStream;
                    Console.WriteLine($"{DateTime.Now}\t" + "Start stream return!");
                    ListenSerialPort(serialPort);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Console.WriteLine(exception.StackTrace);
                }
                finally
                {
                    Console.WriteLine($"{DateTime.Now}\t" + "Serial Port close");
                }
            }
            catch
            {
                Console.WriteLine($"{DateTime.Now}\t" + $"{serialPort.PortName} is busy, opening is impossible");
            }
        }

        private void ListenSerialPort(SerialPort serialPort)
        {
            while (_isActive)
            {
                var buffer = new byte[1024];

                if (serialPort.BytesToRead > 0)
                {
                    Thread.Sleep(serialPort.ReadTimeout);

                    var count = serialPort.BytesToRead;
                    serialPort.Read(buffer, 0, count);

                    ReadData(buffer);
                }

                if (_toWrite.Length > 0)
                {
                    serialPort.Write(_toWrite, 0, _toWrite.Length);
                    _toWrite = new byte[] { };
                }
            }
        }

        private void InitTcpIpPort(TcpIpConnection tcpIp)
        {
            var ipEndPoint = tcpIp.IpEndPoint;
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveBufferSize = 1024;
            socket.SendBufferSize = 1024;
            socket.ReceiveTimeout = 500;
            socket.SendTimeout = 500;

            Console.WriteLine($"{DateTime.Now}\t" + $"Клиент запущен. Попытка соединения с сервером {ipEndPoint}...");
            socket.Connect(ipEndPoint.Address, ipEndPoint.Port);
            Console.WriteLine($"{DateTime.Now}\t" + $"Соединение успешно! Клиент {socket.LocalEndPoint}.");

            ListenTcpIpPort(socket);
        }

        private void ListenTcpIpPort(Socket socket)
        {
            while (socket.Connected
                   &&
                   _isActive)
            {
                Thread.Sleep(socket.ReceiveTimeout);

                var buffer = new byte[1024];
                try
                {
                    if (socket.Receive(buffer) is { } count
                        &&
                        count > 0)
                    {
                        ReadData(buffer);
                    }

                    if (_toWrite.Length > 0)
                    {
                        socket.Send(_toWrite);
                        _toWrite = new byte[] { };
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now}\t" + exception.Message);
                    Console.WriteLine($"{DateTime.Now}\t" + exception.StackTrace);
                }
            }
        }

        private void InitFileSystemFolder(FileSystemConnection fileSystem)
        {
            Console.WriteLine($"{DateTime.Now}\t" + $"{nameof(BaseDriver)}.{nameof(BaseSession)}.{nameof(InitFileSystemFolder)}");
            var pathRead = $"Devices{Path.DirectorySeparatorChar}" +
                           $"{nameof(BaseSession)}{Path.DirectorySeparatorChar}" +
                           $"Read{Path.DirectorySeparatorChar}";
            var pathWrite = $"Devices{Path.DirectorySeparatorChar}" +
                            $"{nameof(BaseSession)}{Path.DirectorySeparatorChar}" +
                            $"Write{Path.DirectorySeparatorChar}";

            Directory.CreateDirectory($"{pathRead}{Path.DirectorySeparatorChar}ToRead");
            Directory.CreateDirectory($"{pathRead}{Path.DirectorySeparatorChar}Readed");
            
            Directory.CreateDirectory($"{pathWrite}{Path.DirectorySeparatorChar}ToWrite");

            while (_isActive)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));

                var files = new DirectoryInfo($"{pathRead}{Path.DirectorySeparatorChar}ToRead").GetFiles();
                foreach (var file in files.OrderBy(x => x.LastWriteTime))
                {
                    Console.WriteLine($"{DateTime.Now}\t" + $"{file.Name} read!");
                    ReadData(Encoding.UTF8.GetBytes(File.ReadAllText(file.FullName)));
                    File.Move(file.FullName, $"{pathRead}{Path.DirectorySeparatorChar}" +
                                             $"Readed{Path.DirectorySeparatorChar}" +
                                             $"{file.Name}");

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }

                if (_toWrite.Length > 0)
                {
                    File.WriteAllText($"{pathWrite}{Path.DirectorySeparatorChar}{DateTime.Now:yyyyMMddHHmmss}", Encoding.UTF8.GetString(_toWrite));
                    _toWrite = new byte[] { };
                }
            }
        }

        public virtual void ReadData(byte[] bytes)
        {
            Console.WriteLine($"{DateTime.Now}\t" + $"{nameof(BaseSession)}.{nameof(ReadData)}(byte[])");
            Console.WriteLine($"{DateTime.Now}\t" + $"{Encoding.UTF8.EncodingName}");
            Console.WriteLine($"{DateTime.Now}\t" + $"{Encoding.UTF8.GetString(bytes)}");
        }

        public void WriteData(byte[] bytes)
        {
            Console.WriteLine($"{DateTime.Now}\t" + $"{nameof(BaseSession)}.{nameof(WriteData)}(byte[])");
            _toWrite = bytes;
        }
    }
}