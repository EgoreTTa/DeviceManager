namespace DeviceManager.Entities
{
    using Configurations.Device;
    using Configurations.Device.Connection;
    using DataAccess;
    using DriverBase;
    using Serilog;
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Device
    {
        public ILogger Logger { get; set; }
        public DeviceConfiguration Configuration { get; set; }
        private DataAccess _dataAccess;

        public async Task StartAsync(string url, IParser parser, CancellationToken token)
        {
            _dataAccess = new DataAccess(url, Configuration.SystemName, Configuration.DriverConfiguration.SystemName);
            parser.Encoding = Encoding.GetEncoding(Configuration.DriverConfiguration.Encoding);
            Logger.Information($"Encoding:{parser.Encoding.BodyName}");

            Logger.Information($"ConnectionType:{Configuration.ConnectionConfiguration.ConnectionType}");
            switch (Configuration.ConnectionConfiguration.ConnectionType)
            {
                case ConnectionTypes.Network:
                    await ListenerNetwork(Configuration.ConnectionConfiguration.Network, parser, token);
                    break;
                case ConnectionTypes.Serial:
                    await ListenerSerialPort(Configuration.ConnectionConfiguration.Serial, parser, token);
                    break;
                case ConnectionTypes.FileSystem:
                    await ListenerFileSystem(Configuration.ConnectionConfiguration.FileSystem, parser, token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task ListenerNetwork(NetworkConnection connection, IParser parser, CancellationToken token)
        {
            if (connection.Mode == NetworkModes.Server)
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Loopback, connection.Port);

                using var server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
                server.Bind(ipEndPoint);
                server.Listen(1);

                Logger.Debug($"network {ipEndPoint.Address}:{ipEndPoint.Port} socket server create!");

                var buffer = new byte[2048];
                while (token.IsCancellationRequested is false)
                {
                    var client = await server.AcceptAsync();

                    Logger.Information($"network {client.RemoteEndPoint} socket server connect!");

                    while (client.Connected)
                    {
                        try
                        {
                            var count = await client.ReceiveAsync(buffer, SocketFlags.None, token);
                            var bytes = buffer.Take(count).ToArray();
                            Logger.Debug($"network receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                            parser.Parse(bytes, out var samples, out var send);
                            if (send != null)
                            {
                                await client.SendAsync(send, SocketFlags.None, token);
                                Logger.Debug($"network send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                            }

                            if (samples != null)
                                if (samples.Any(x => x.Results != null))
                                {
                                    await _dataAccess.SetDeviceResults(samples);
                                }
                                else
                                {
                                    var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                                        samples.Select(x => x.SampleCode)
                                               .ToArray());
                                    parser.ParseOrder(directiveLines, out var order);

                                    await client.SendAsync(send, SocketFlags.None, token);
                                    Logger.Debug($"network send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                                }
                        }
                        catch (Exception exception)
                        {
                            Logger.Fatal("network", exception);
                            Logger.Error("network", exception);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }

                        Logger.Warning($"network {client.RemoteEndPoint} end!");
                    }

                    //todo socket.close or socket.disconnect or socket.shutdown?
                }
            }
            else
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Parse(connection.Address), connection.Port);
                using var client = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);

                Logger.Warning($"network {ipEndPoint.Address}:{ipEndPoint.Port} socket client create!");

                var buffer = new byte[2048];
                while (token.IsCancellationRequested is false)
                { 
                    await client.ConnectAsync(ipEndPoint);

                    Logger.Warning($"network {client.RemoteEndPoint} socket client connect!");
                    
                    while (client.Connected)
                    {
                        try
                        {
                            var count = await client.ReceiveAsync(buffer, SocketFlags.None, token);
                            var bytes = buffer.Take(count).ToArray();
                            Logger.Warning($"network receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                            parser.Parse(bytes, out var samples, out var send);
                            if (send != null)
                            {
                                await client.SendAsync(send, SocketFlags.None, token);
                                Logger.Warning($"network send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                            }

                            if (samples != null)
                                if (samples.Any(x => x.Results != null))
                                {
                                    await _dataAccess.SetDeviceResults(samples);
                                }
                                else
                                {
                                    var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                                        samples.Select(x => x.SampleCode)
                                               .ToArray());
                                    parser.ParseOrder(directiveLines, out var order);

                                    await client.SendAsync(send, SocketFlags.None, token);
                                }
                        }
                        catch (Exception exception)
                        {
                            Logger.Fatal("network", exception);
                            Logger.Error("network", exception);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }

                        Logger.Warning($"network {client.RemoteEndPoint} close!");
                    }

                    //todo socket.close or socket.disconnect or socket.shutdown?
                }
            }
        }

        private async Task ListenerSerialPort(SerialConnection connection, IParser parser, CancellationToken token)
        {
            var serial = new SerialPort($"{connection.PortName}");
            serial.Open();
            Logger.Information($"serial {connection.PortName} open!");

            var buffer = new byte[2048];
            while (token.IsCancellationRequested is false)
            {
                try
                {
                    var count = await serial.BaseStream.ReadAsync(buffer, token);
                    var bytes = buffer.Take(count).ToArray();
                    Logger.Debug($"serial {connection.PortName} receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                    parser.Parse(bytes, out var samples, out var send);
                    if (send != null)
                    {
                        await serial.BaseStream.WriteAsync(send, token);
                        Logger.Debug($"serial {connection.PortName} send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                    }

                    if (samples != null)
                        if (samples.Any(x => x.Results != null))
                        {
                            await _dataAccess.SetDeviceResults(samples);
                        }
                        else
                        {
                            var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                                samples.Select(x => x.SampleCode)
                                       .ToArray());
                            parser.ParseOrder(directiveLines, out var order);

                            await serial.BaseStream.WriteAsync(order, token);
                            Logger.Debug($"serial {connection.PortName} send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                        }
                }
                catch (Exception exception)
                {
                    Logger.Fatal("serial", exception);
                    Logger.Error("serial", exception);
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            }
            serial.Close();
            Logger.Warning($"serial {connection.PortName} close!");
        }

        private async Task ListenerFileSystem(FileSystemConnection connection, IParser parser, CancellationToken token)
        {
            while (token.IsCancellationRequested is false)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                var files = Directory.GetFiles(connection.FolderToRead);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            var bytes = await File.ReadAllBytesAsync(file, token);
                            Logger.Warning($"filesystem {connection.FolderToRead}{Path.DirectorySeparatorChar}{file} receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                            parser.Parse(bytes, out var samples, out var send);
                            if (send != null)
                            {
                                await File.WriteAllBytesAsync(connection.FolderToWrite, send, token);
                                Logger.Warning($"filesystem {connection.FolderToWrite} send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                            }

                            if (samples != null)
                                if (samples.Any(x => x.Results != null))
                                {
                                    await _dataAccess.SetDeviceResults(samples);
                                }
                                else
                                {
                                    var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                                        samples.Select(x => x.SampleCode)
                                               .ToArray());
                                    parser.ParseOrder(directiveLines, out var order);

                                    await File.WriteAllBytesAsync(connection.FolderToWrite, send, token);
                                }
                        }
                        catch (Exception exception)
                        {
                            Logger.Fatal("filesystem", exception);
                            Logger.Error("filesystem", exception);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken token) { }
    }
}