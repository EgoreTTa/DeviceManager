namespace DeviceManagerAPI.Controllers.Devices.Services
{
    using DeviceManagerWorker;
    using Forms;
    using System.Linq;
    using System.Threading.Tasks;

    public class DevicesControllerService : IDevicesControllerService
    {
        public async Task AddDevice(DeviceForm deviceForm)
        {
            var devices = await Worker.GetDevices();

            var newId = devices.Length > 0 ? devices.Select(device => device.Id).Max() + 1 : 0;

            await Worker.AddDevice(new Device()
            {
                Id = newId,
                Name = deviceForm.Name,
                SystemName = deviceForm.SystemName,
            });
        }

        public async Task RemoveDevice(int id)
        {
            await Worker.RemoveDevice(id);
        }

        public async Task<DeviceForm[]> GetDevices()
        {
            var devices = await Worker.GetDevices();
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