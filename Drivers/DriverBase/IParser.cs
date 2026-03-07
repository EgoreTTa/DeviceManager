namespace DriverBase
{
    using DataAccess.DTOs;
    using System.Text;
    using Serilog;

    public interface IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; }
        public void Clear();
        public void Parse(byte[] bytes, out TestResult[] samples, out byte[] send);
        public void ParseOrder(DeviceOrderDTO[] directiveLines, out byte[] send);
    }
}