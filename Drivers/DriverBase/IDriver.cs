namespace DriverBase
{
    using System.Collections.Generic;
    using Serilog;

    public interface IDriver
    {
        public ILogger Logger { get; set; }
        public string[] SupportedDevices { get; }
        public Dictionary<IParser, string> Parsers { get; }
    }
}