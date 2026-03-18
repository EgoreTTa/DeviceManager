namespace Core.Configurations.Device.Connection
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;

    public class NetworkConnection : IEquatable<NetworkConnection>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NetworkModes Mode { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Mode, Address, Port);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NetworkConnection)obj);
        }

        public bool Equals(NetworkConnection other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Mode == other.Mode && Address == other.Address && Port == other.Port;
        }
    }
}