namespace DriverBase
{
    using DTOs;
    using Infrastructure.DTOs.LIS;
    using Serilog;
    using System.Text;
    using System.Threading.Tasks;

    public interface IParser
    {
        ILogger Logger { get; set; }
        Encoding Encoding { get; set; }
        void Clear();
        OptionDTO[] GetOptions() => new OptionDTO[] { };
        void SetOptions(OptionDTO[] options) { }
        Task<ParserMessage> WriteAsync(byte[] bytes);
        Task<ParserMessage> ReadAsync();
        Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders);
    }
}