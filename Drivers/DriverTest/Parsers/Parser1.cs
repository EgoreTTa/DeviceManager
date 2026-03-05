namespace DriverTest.Parsers
{
    using DataAccess.DTOs;
    using DriverBase;
    using Serilog;
    using System.Text;

    public sealed class Parser1 : IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public void Clear()
        {
            Logger.Warning("Parser clear...");
        }

        public bool TryParse(byte[] bytes, out TestResult[] samples, out byte[] send)
        {
            Logger.Information(Encoding.GetString(bytes));

            samples = new[]
            {
                new TestResult
                {
                    SampleCode = "05032026-6767",
                    Results = new[]
                    {
                        new Result
                        {
                            TestCode = "TBil",
                            Value = "10.0",
                            MuCode = "g/l"
                        }
                    }
                },
            };
            send = null;

            return false;
        }

        public bool TryParseOrder(DeviceOrderDTO[] directiveLines, out byte[] send)
        {
            send = null;
            return false;
        }
    }
}