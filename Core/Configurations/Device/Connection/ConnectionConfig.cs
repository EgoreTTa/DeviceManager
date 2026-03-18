namespace Core.Configurations.Device.Connection
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ConnectionConfig : IEquatable<ConnectionConfig>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ConnectionTypes ConnectionType { get; set; }
        public NetworkConnection Network { get; set; }
        public SerialConnection Serial { get; set; }
        public FileSystemConnection FileSystem { get; set; }

        public bool Equals(ConnectionConfig other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ConnectionType == other.ConnectionType && Equals(Network, other.Network) && Equals(Serial, other.Serial) && Equals(FileSystem, other.FileSystem);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConnectionConfig)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)ConnectionType, Network, Serial, FileSystem);
        }
    }
}