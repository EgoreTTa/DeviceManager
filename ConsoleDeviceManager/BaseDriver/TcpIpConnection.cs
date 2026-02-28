namespace BaseDriver
{
    using System.Net;

    public struct TcpIpConnection
    {
        public TcpIpModes Mode { get; set; }
        public IPEndPoint IpEndPoint { get; set; }
    }
}