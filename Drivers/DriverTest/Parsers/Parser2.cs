namespace DriverTest.Parsers
{
    using DataAccess.DTOs;
    using DriverBase;
    using Serilog;
    using System.Text;

    public sealed class Parser2 : IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;
        
        public void Clear()
        {
            Logger.Warning("Parser clear...");
        }

        public bool TryParse(byte[] bytes, out TestResult[] samples, out byte[] send)
        {
            samples = new[]
            {
                new TestResult
                {
                    SampleCode = "05032026-45454",
                    Results = new[]
                    {
                        new Result
                        {
                            TestCode = "Pro",
                            Value = "11.0",
                            MuCode = "mg/ml"
                        }
                    }
                },
            };
            send = Encoding.GetBytes("\x05");

            return true;
        }

        public bool TryParseOrder(DeviceOrderDTO[] directiveLines, out byte[] send)
        {
            send = Encoding.GetBytes("\x02" + "05032026-45454" + "\x03");
            return true;
        }
    }
}
