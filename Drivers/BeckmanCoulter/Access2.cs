namespace BeckmanCoulter
{
    using DriverBase;
    using Infrastructure.DTOs.LIS;
    using Serilog;
    using System.Text;
    using System.Threading.Tasks;

    public class Access2 : IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; }
        public void Clear()
        {
            throw new System.NotImplementedException();
        }
        public Task<ParserMessage> WriteAsync(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
        public Task<ParserMessage> ReadAsync()
        {
            throw new System.NotImplementedException();
        }
        public Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders)
        {
            throw new System.NotImplementedException();
        }
    }
}
