namespace DriverBase
{
    using DTOs;

    public class ParserMessage
    {
        public byte[] ForConnect { get; set; }
        public TestResultDTO[] ForDeviceService { get; set; }
    }
}