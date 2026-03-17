using System.Threading.Tasks;

namespace DriverTest2
{
    using DriverBase.DTOs;
    using DataAccess.DTOs.LIS;
    using DriverBase;
    using Serilog;
    using System.Text;

    public sealed class Custom : IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public void Clear()
        {
            Logger.Warning("Parser clear...");
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

        public void Parse(byte[] bytes, out TestResultDTO[] samples, out byte[] send)
        {
            var data = Encoding.GetString(bytes);
            Logger.Debug($" <-:<{GetMessageForLogger(data)}>");
            samples = null;
            send = new byte[] { 6 };
            Logger.Debug($" ->:<{GetMessageForLogger(data)}>");
        }

        public void ParseOrder(DeviceOrderDTO[] directiveLines, out byte[] send)
        {
            send = Encoding.GetBytes("\x02" + "05032026-45454" + "\x03");
        }

        private static string GetMessageForLogger(string message)
        {
            return message.Replace($"\x1", "<SOH>")
                          .Replace($"\x2", "<STX>")
                          .Replace($"\x3", "<ETX>")
                          .Replace($"\x4", "<EOT>")
                          .Replace($"\x5", "<ENQ>")
                          .Replace($"\x6", "<ACK>")
                          .Replace($"\x15", "<NAK>")
                          .Replace($"\x17", "<ETB>")
                          .Replace($"\x0A", "<LF>")
                          .Replace($"\x0D", "<CR>");
        }
    }
}