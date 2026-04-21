namespace Core.Devices.Components.Connect
{
    using Core.Configurations.Device.Connection;
    using Serilog;
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class SerialConnect : IConnection, IDisposable
    {
        private readonly SerialPort _serialPort;

        public ILogger Logger { get; set; }

        public SerialConnect(ILogger logger, SerialConnection configuration)
        {
            Logger = logger;
            _serialPort = new SerialPort(
                configuration.PortName, 
                configuration.BaudRate, 
                configuration.Parity, 
                configuration.DataBits, 
                configuration.StopBits);
        }

        public Task StartAsync(CancellationToken token)
        {
            OpenPort();
            token.Register(ClosePort);
            return Task.CompletedTask;
        }

        private void OpenPort()
        {
            try
            {
                Logger.Debug($"serial {_serialPort.PortName} open...");
                _serialPort.Open();
                Logger.Debug($"serial {_serialPort.PortName} opened.");
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
        }

        private void ClosePort()
        {
            try
            {
                Logger.Debug($"serial {_serialPort.PortName} close...");
                _serialPort.Close();
                Logger.Debug($"serial {_serialPort.PortName} closed.");
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
        }

        public void Stop()
        {
            _serialPort.Close();
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
                switch (exception)
                {
                    case IOException { }:
                        break;
                    default:
                        OpenPort();
                        break;
                }

                throw exception;
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
                OpenPort();
                throw exception;
            }
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}