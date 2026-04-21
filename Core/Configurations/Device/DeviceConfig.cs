namespace Core.Configurations.Device
{
    using Connection;
    using DriverBase.DTOs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Serilog;
    using Serilog.Events;
    using System;

    public class DeviceConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string DriverSystemName { get; set; }
        public bool IsActive { get; set; }
        public ParserConfig Parser { get; set; }
        public ConnectionConfig Connection { get; set; }
        public LoggerConfig Logger { get; set; }
    }

    public class ParserConfig : IEquatable<ParserConfig>
    {
        public string FullName { get; set; }
        public string Encoding { get; set; }
        public OptionDTO[] Options { get; set; }
        public LoggerConfig Logger { get; set; }

        public bool Equals(ParserConfig other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return FullName == other.FullName 
                   && 
                   Encoding == other.Encoding 
                   && 
                   Equals(Options, other.Options) 
                   && 
                   Equals(Logger, other.Logger);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ParserConfig)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FullName, Encoding, Options, Logger);
        }
    }

    public class LoggerConfig : IEquatable<LoggerConfig>
    {
        public string Path { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public LogEventLevel RestrictedToMinimumLevel { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RollingInterval RollingInterval { get; set; }
        public long? FileSizeLimitBytes { get; set; }
        public bool RollOnFileSizeLimit { get; set; }
        public int? RetainedFileCountLimit { get; set; }
        public bool Shared { get; set; }

        public bool Equals(LoggerConfig other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Path == other.Path
                   &&
                   RestrictedToMinimumLevel == other.RestrictedToMinimumLevel
                   &&
                   RollingInterval == other.RollingInterval
                   &&
                   FileSizeLimitBytes == other.FileSizeLimitBytes
                   &&
                   RollOnFileSizeLimit == other.RollOnFileSizeLimit
                   &&
                   RetainedFileCountLimit == other.RetainedFileCountLimit
                   &&
                   Shared == other.Shared;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LoggerConfig)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, (int)RestrictedToMinimumLevel, (int)RollingInterval, FileSizeLimitBytes, RollOnFileSizeLimit, RetainedFileCountLimit, Shared);
        }
    }
}