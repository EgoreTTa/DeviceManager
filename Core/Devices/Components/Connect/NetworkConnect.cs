namespace Core.Devices.Components.Connect
{
    using Core.Configurations.Device.Connection;
    using Serilog;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class NetworkConnect : IConnection, IDisposable
    {
        private readonly NetworkModes _mode;
        private readonly string _address;
        private readonly int _port;

        private Socket _server;
        private Socket _socket;
        private Socket _client;

        public ILogger Logger { get; set; }

        public NetworkConnect(ILogger logger, NetworkConnection configuration)
        {
            Logger = logger;
            _mode = configuration.Mode;
            _address = configuration.Address;
            _port = configuration.Port;
        }

        public Task StartAsync(CancellationToken token)
        {
            token.Register(Stop);

            if (_mode == NetworkModes.Server)
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Loopback, _port);

                _socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
                _socket.Bind(ipEndPoint);
                _socket.Listen(1);

                Logger.Information($"network server {ipEndPoint.Address}:{ipEndPoint.Port} socket create!");
            }
            else
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Parse(_address), _port);
                _client = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);

                Logger.Information($"network client {ipEndPoint.Address}:{ipEndPoint.Port} socket create!");
            }

            return Task.CompletedTask;
            // => SocketConnect();
        }

        public void Stop()
        {
            _server?.Close();
            _client?.Close();
            _socket?.Close();

            Logger.Information($"network connect stop.");
        }

        private async Task SocketConnect()
        {
            if (_mode == NetworkModes.Server)
            {
                Logger.Debug($"DeviceConnection await _socket.AcceptAsync...");
                _server = await _socket.AcceptAsync();
                Logger.Debug($"DeviceConnection AcceptAsync!");
                var iPEndPoint = _server.RemoteEndPoint as IPEndPoint;
                Logger.Information($"network client {iPEndPoint?.Address}:{iPEndPoint?.Port} socket accept!");
            }
            else
            {
                Logger.Debug($"DeviceConnection await _client.ConnectAsync...");
                await _client.ConnectAsync(new IPEndPoint(IPAddress.Parse(_address), _port));
                Logger.Debug($"DeviceConnection ConnectAsync!");
                var iPEndPoint = _client.RemoteEndPoint as IPEndPoint;
                Logger.Information($"network client {iPEndPoint?.Address}:{iPEndPoint?.Port} socket connect!");
            }
        }

        public async Task<byte[]> ReadAsync(CancellationToken token)
        {
            try
            {
                var buffer = new byte[2048];
                var socket = _server ?? _client;
                var count = await socket.ReceiveAsync(buffer, SocketFlags.None, token);
                var result = buffer.Take(count).ToArray();
                Logger.Debug($"network receive: {string.Join(", ", result.Select(x => $"{x:X2}"))}");
                return result;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                if (exception is TaskCanceledException) throw exception;
                // if (exception is NullReferenceException) throw exception;
                await SocketConnect();
                throw new Exception("No connection, reconnection was performed");
            }
        }

        public async Task WriteAsync(byte[] bytes, CancellationToken token)
        {
            try
            {
                var socket = _server ?? _client;
                await socket.SendAsync(bytes, SocketFlags.None, token);
                Logger.Debug($"network send: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                if (exception is TaskCanceledException) throw exception;
                // if (exception is NullReferenceException) throw exception;
                await SocketConnect();
                throw new Exception("No connection, reconnection was performed");
            }
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _client?.Dispose();
            _server?.Dispose();
        }
    }
}