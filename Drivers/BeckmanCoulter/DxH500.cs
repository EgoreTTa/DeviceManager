namespace BeckmanCoulter
{
    using DriverBase;
    using Infrastructure.DTOs.LIS;
    using Serilog;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class DxH500 : IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        public Task<ParserMessage> WriteAsync(byte[] bytes)
        {
            throw new NotImplementedException();
        }
        public Task<ParserMessage> ReadAsync()
        {
            throw new NotImplementedException();
        }
        public Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders)
        {
            throw new NotImplementedException();
        }
    }
}
