namespace DeviceManagerWorker
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataAccess;

    public class Driver : IDriver
    {
        private int _id;
        private bool _isActive;

        public int Id => _id;
        public bool IsActive => _isActive;
        public Connection Connection { get; set; }
        public DataAccess DataAccess { get; set; }

        public async Task StartAsync(CancellationToken token)
        {
            
        }

        public async Task<byte[]> WriteAsync(byte[] bytes)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Driver receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

            return new byte[] { };
        }
    }
}