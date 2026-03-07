namespace DeviceManagerAPI.Controllers.Devices.Services
{
    using global::DeviceManager.Configurations.Device;
    using global::DeviceManager.Entities;
    using System.Threading.Tasks;

    public interface IDevicesControllerService
    {
        public Task<DeviceManagerEvent> AddDevice(DeviceConfiguration device);
        public Task<DeviceManagerEvent> RemoveDevice(int id);
        public Task<DeviceConfiguration[]> GetDevices();
        public Task<DeviceConfiguration> GetDevice(int id);
        public Task<DeviceManagerEvent> UpdateDevice(int id, DeviceConfiguration device);
        public Task<DeviceManagerEvent> FlipActive(int id);
    }
}