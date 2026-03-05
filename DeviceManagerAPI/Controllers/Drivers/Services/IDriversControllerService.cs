namespace DeviceManagerAPI.Controllers.Drivers.Services
{
    using DeviceManagerService.Configurations.Device.Driver;
    using System.Threading.Tasks;

    public interface IDriversControllerService
    {
        public Task AddDriver(DriverConfiguration driver);
        public Task RemoveDriver(int id);
        public Task<DriverConfiguration[]> GetDrivers();
        public Task<DriverConfiguration> GetDriver(int id);
    }
}