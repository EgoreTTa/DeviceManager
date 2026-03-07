namespace DeviceManagerService.Services
{
    using Configurations.Device;
    using Configurations.Device.Driver;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDeviceManagerUseService
    {
        public Task StartAsync(CancellationToken token);
        public Task<DeviceConfiguration[]> GetDevices(CancellationToken token = default);
        public Task<DeviceConfiguration> GetDevice(int id, CancellationToken token = default);
        public Task<Driver[]> GetDrivers(CancellationToken token = default);
        public Task<DriverConfiguration> GetDrive(int id, CancellationToken token = default);
        public Task AddDevice(DeviceConfiguration device, CancellationToken token = default);
        public Task RemoveDevice(int id, CancellationToken token = default);
        public Task UpdateDevice(int id, DeviceConfiguration device, CancellationToken token = default);
    }
}