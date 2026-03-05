namespace DeviceManagerService.Others
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDriverOld
    {
        public int Id { get; }
        public bool IsActive { get; }
        public Connection Connection { get; set; }

        public Task StartAsync(CancellationToken token);
        public Task<byte[]> WriteAsync(byte[] bytes);
    }
}