namespace DeviceManager.Devices.Components.Connect
{
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;

    public interface IConnection
    {
        public ILogger Logger { get; set; }

        Task StartAsync(CancellationToken token);
        Task StopAsync(CancellationToken token);
        Task<byte[]> ReadAsync(CancellationToken token);
        Task WriteAsync(byte[] bytes, CancellationToken token);
    }
}