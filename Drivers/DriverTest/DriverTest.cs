namespace DriverTest
{
    using DriverBase;
    using Serilog;
    using Parsers;
    using System.Collections.Generic;

    public sealed class Driver : IDriver
    {
        private ILogger _logger;

        public ILogger Logger
        {
            get => _logger;
            set
            {
                _logger = value;
                foreach (var parser in Parsers)
                {
                    parser.Key.Logger = _logger;
                }

                _logger.Information("Driver initialisation!");
            }
        }

        public string[] SupportedDevices { get; } = new string[]
        {
            "TestDevice"
        };

        public Dictionary<IParser, string> Parsers { get; } = new Dictionary<IParser, string>()
        {
            [new Parser1()] = "Test Parser1",
            [new Parser2()] = "Test Parser2",
        };
    }
}