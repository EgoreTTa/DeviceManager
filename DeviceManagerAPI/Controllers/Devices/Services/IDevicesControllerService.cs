namespace DeviceManagerAPI.Controllers.Devices.Services
{
    using Forms;
    using System.Threading.Tasks;

    public interface IDevicesControllerService
    {
        public Task AddDevice(DeviceForm deviceForm);
        public Task RemoveDevice(int id);
        public Task<DeviceForm[]> GetDevices();
    }
}