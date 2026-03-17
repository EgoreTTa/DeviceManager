namespace DeviceManagerHubService
{
    using System.Collections.Generic;

    public interface IDeviceManagerHubService
    {
        public void AddLog(string message);
        public IEnumerable<string> GetHistory();
    }
}