namespace Core.Devices.Components.Connect
{
    using Core.Configurations.Device.Connection;
    using Serilog;
    using System;
    using System.IO.Ports;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class SerialConnect : IConnection, IDisposable
    {
        private readonly SerialPort _serialPort;

        private Task _task;
        private CancellationTokenSource _source;

        public ILogger Logger { get; set; }

        public SerialConnect(ILogger logger, SerialConnection configuration)
        {
            Logger = logger;
            _serialPort = new SerialPort($"{configuration.PortName}");
        }

        public Task StartAsync(CancellationToken token)
        {
            Logger.Debug($"serial {_serialPort.PortName} open...");
            _serialPort.Open();
            Logger.Debug($"serial {_serialPort.PortName} opened.");
            token.Register(() => _serialPort.Close());

            return Task.CompletedTask;
        }

        public void Stop()
        {
            _source.Cancel();

            _serialPort.Close();

            _task.Wait();
        }

        public async Task<byte[]> ReadAsync(CancellationToken token)
        {
            try
            {
                var buffer = new byte[2048];
                var count = await _serialPort.BaseStream.ReadAsync(buffer, token);
                var bytes = buffer.Take(count).ToArray();
                Logger.Debug($"serial {_serialPort.PortName} receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");
                return bytes;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                throw new NotImplementedException();
            }
        }

        public async Task WriteAsync(byte[] bytes, CancellationToken token)
        {
            try
            {
                await _serialPort.BaseStream.WriteAsync(bytes, token);
                Logger.Debug($"serial {_serialPort.PortName} send: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}