namespace DeviceManagerAPI.Controllers.Devices.Services
{
    using DeviceManagerService.Configurations.Device;
    using DeviceManagerService.Services;
    using System.Linq;
    using System.Threading.Tasks;

    public class DevicesControllerService : IDevicesControllerService
    {
        private readonly IDeviceManagerUseService _useCase;

        public DevicesControllerService(IDeviceManagerUseService useCase)
        {
            _useCase = useCase;
        }

        public async Task AddDevice(DeviceConfiguration device)
        {
            var devices = await _useCase.GetDevices();

            var newId = devices.Max(x => x.Id) + 1;
            device.Id = newId;

            await _useCase.AddDevice(device);
        }

        public async Task UpdateDevice(int id, DeviceConfiguration device)
        {
            await _useCase.UpdateDevice(id, device);
        }

        public async Task RemoveDevice(int id)
        {
            await _useCase.RemoveDevice(id);
        }

        public async Task<DeviceConfiguration[]> GetDevices()
        {
            var devices = await _useCase.GetDevices();
            return devices.OrderBy(x=>x.Id).ToArray();
        }

        public async Task<DeviceConfiguration> GetDevice(int id) => await _useCase.GetDevice(id);
    }
}