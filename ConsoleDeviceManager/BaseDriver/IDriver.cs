namespace BaseDriver
{
    using System.Net;
    using System.Text;

    public interface IDriver
    {
        public void SetDevice(string name, string systemName);
        public void SetDriver(string name, string systemName, Encoding encoding);
        public void SetConnectionType(int type);
        public void SetConnectionSettings(
            string portName, 
            int baudRate,
            int parity,
            int dataBits,
            int stopBits);
        public void SetConnectionSettings(string mode, IPEndPoint endPoint);
        public void SetConnectionSettings(
            string folderToRead, 
            string folderToWrite);

        public void SetRegistry(DeviceRegistry deviceRegistry);
        public void Start();
        public void Stop();
    }
}