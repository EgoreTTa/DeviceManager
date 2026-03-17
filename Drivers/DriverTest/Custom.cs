namespace DriverTest
{
    using DataAccess.DTOs.LIS;
    using DriverBase;
    using Serilog;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class Custom : IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public void Clear()
        {
            Logger.Warning("Parser clear...");
        }

        Task<ParserMessage> IParser.WriteAsync(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
        Task<ParserMessage> IParser.ReadAsync()
        {
            throw new System.NotImplementedException();
        }
        public Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders)
        {
            throw new System.NotImplementedException();
        }
    }
}