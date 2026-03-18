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
        private readonly Socket _socket;
        private readonly Socket _client;
        private readonly IPEndPoint _ipEndPoint;

        private Socket _server;
        private CancellationTokenSource _source;

        public ILogger Logger { get; set; }

        public NetworkConnect(ILogger logger, NetworkConnection configuration)
        {
            Logger = logger;

            if (configuration.Mode == NetworkModes.Server)
            {
                _ipEndPoint = new IPEndPoint(IPAddress.Loopback, configuration.Port);

                _socket = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
                _socket.Bind(_ipEndPoint);
                _socket.Listen(1);

                Logger.Information($"network {_ipEndPoint.Address}:{_ipEndPoint.Port} socket server create!");
            }
            else
            {
                _ipEndPoint = new IPEndPoint(IPAddress.Parse(configuration.Address), configuration.Port);
                _client = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);

                Logger.Information($"network {_ipEndPoint.Address}:{_ipEndPoint.Port} socket client create!");
            }
        }

        private async Task SocketConnect()
        {
            if (_socket is { })
            {
                Logger.Debug($"DeviceConnection await _socket.AcceptAsync...");
                _server = await _socket.AcceptAsync();
                Logger.Debug($"DeviceConnection AcceptAsync!");
            }

            if (_client is { })
            {
                Logger.Debug($"DeviceConnection await _client.ConnectAsync...");
                await _client.ConnectAsync(_ipEndPoint);
                Logger.Debug($"DeviceConnection ConnectAsync!");
            }
        }

        public Task StartAsync(CancellationToken token) => SocketConnect();

        public void Stop()
        {
            _source.Cancel();

            _socket?.Close();
            _server?.Close();
            _client?.Close();

            Logger.Information($"network connect stop.");
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
                await SocketConnect();
                throw new Exception("No connection, reconnection was performed");
            }
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _server?.Dispose();
            _client?.Dispose();
        }
    }
}