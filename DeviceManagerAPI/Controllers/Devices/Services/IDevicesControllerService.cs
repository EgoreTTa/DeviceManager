namespace DeviceManagerAPI.Controllers.Devices.Services
{
    using DeviceManagerService.Configurations.Device;
    using System.Threading.Tasks;

    public interface IDevicesControllerService
    {
        public Task AddDevice(DeviceConfiguration device);
        public Task RemoveDevice(int id);
        public Task<DeviceConfiguration[]> GetDevices();
        public Task<DeviceConfiguration> GetDevice(int id);
        public Task UpdateDevice(int id, DeviceConfiguration device);
    }
}