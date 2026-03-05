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
        public bool TryParse(byte[] bytes, out TestResult[] samples, out byte[] send);
        public bool TryParseOrder(DeviceOrderDTO[] directiveLines, out byte[] send);
    }
}