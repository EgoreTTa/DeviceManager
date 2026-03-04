namespace DeviceManagerAPI.Controllers.Devices.Services
{
    using Forms;
    using System.Threading.Tasks;
    using DeviceManagerService.Services;
    using System.Linq;
    using DeviceManagerService.Others;

    public class DevicesControllerService : IDevicesControllerService
    {
        private readonly IDeviceManagerUseService _useCase;

        public DevicesControllerService(IDeviceManagerUseService useCase)
        {
            _useCase = useCase;
        }

        public async Task AddDevice(DeviceForm deviceForm)
        {
            var devices = await _useCase.GetDevices();

            var newId = devices.Length > 0 ? devices.Select(device => device.Id).Max() + 1 : 0;

            await _useCase.AddDevice(new Device()
            {
                Id = newId,
                Name = deviceForm.Name,
                SystemName = deviceForm.SystemName,
            });
        }

        public async Task RemoveDevice(int id)
        {
            await _useCase.RemoveDevice(id);
        }

        public async Task<DeviceForm[]> GetDevices()
        {
            var devices = await _useCase.GetDevices();
            return devices.Select(device => new DeviceForm()
                          {
                              Id = device.Id,
                              Name = device.Name,
                              SystemName = device.SystemName,
                          })
                          .ToArray();
        }
    }
}