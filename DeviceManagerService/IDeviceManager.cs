namespace DeviceManager
{
    using Configurations;
    using Configurations.Device;
    using Configurations.Device.Driver;
    using Entities;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDeviceManager
    {
        public Task StartAsync(CancellationToken token);
        public Task<DeviceConfiguration[]> GetDevices(CancellationToken token = default);
        public Task<DeviceConfiguration> GetDevice(int id, CancellationToken token = default);
        public Task<Driver[]> GetDrivers(CancellationToken token = default);
        public Task<DeviceManagerEvent[]> GetEvents(CancellationToken token = default);
        public Task<DriverConfiguration> GetDrive(int id, CancellationToken token = default);
        public Task<DeviceManagerEvent> AddDevice(DeviceConfiguration device, CancellationToken token = default);
        public Task<DeviceManagerEvent> RemoveDevice(int id, CancellationToken token = default);
        public Task<DeviceManagerEvent> UpdateDevice(int id, DeviceConfiguration device, CancellationToken token = default);
        public Task<DeviceManagerConfiguration> GetSettings();
        public Task<DeviceManagerEvent> UpdateSettings(DeviceManagerConfiguration formDeviceManagerSettings);
        public Task<DeviceManagerEvent> FlipActive(int id);
    }
}