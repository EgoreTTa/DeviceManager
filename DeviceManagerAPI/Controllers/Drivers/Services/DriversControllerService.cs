namespace DeviceManagerAPI.Controllers.Drivers.Services
{
    using DeviceManagerService.Configurations.Device.Driver;
    using DeviceManagerService.Services;
    using System.Threading.Tasks;

    public class DriversControllerService : IDriversControllerService
    {
        private readonly IDeviceManagerUseService _useCase;

        public DriversControllerService(IDeviceManagerUseService useCase)
        {
            _useCase = useCase;
        }

        public Task AddDriver(DriverConfiguration driver)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveDriver(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<DriverConfiguration[]> GetDrivers()
        {
            return await _useCase.GetDrivers();
        }

        public Task<DriverConfiguration> GetDriver(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}