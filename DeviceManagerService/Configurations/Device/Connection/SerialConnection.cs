namespace DeviceManager.Configurations.Device.Connection
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.IO.Ports;

    public class SerialConnection : IEquatable<SerialConnection>
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Parity Parity { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StopBits StopBits { get; set; }

        public bool Equals(SerialConnection other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return PortName == other.PortName && BaudRate == other.BaudRate && DataBits == other.DataBits && Parity == other.Parity && StopBits == other.StopBits;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SerialConnection)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PortName, BaudRate, DataBits, (int)Parity, (int)StopBits);
        }
    }
}