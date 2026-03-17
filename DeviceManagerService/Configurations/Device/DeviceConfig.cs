namespace DeviceManager.Configurations.Device
{
    using Connection;
    using Driver;
    using DriverBase.DTOs;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Events;

    public class DeviceConfig
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string DriverSystemName { get; set; }
        public bool IsActive { get; set; }
        public ParserConfig Parser { get; set; }
        public DriverConfig Driver { get; set; }
        public ConnectionConfig Connection { get; set; }
        public LoggerConfig Logger { get; set; }
    }

    public class ParserConfig
    {
        public string FullName { get; set; }
        public string Encoding { get; set; }
        public OptionDTO[] Options { get; set; }
        public LoggerConfig Logger { get; set; }
    }

    public class LoggerConfig
    {
        public string Path { get; set; }
        public LogEventLevel RestrictedToMinimumLevel { get; set; }
        public RollingInterval RollingInterval { get; set; }
        public long? FileSizeLimitBytes { get; set; }
        public bool RollOnFileSizeLimit { get; set; }
        public int? RetainedFileCountLimit { get; set; }
        public bool Shared { get; set; }
    }

    public class DeviceConfigBackup
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string DeviceConfig { get; set; }
    }
}