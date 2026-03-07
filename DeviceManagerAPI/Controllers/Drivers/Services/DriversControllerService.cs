namespace DeviceManagerAPI.Controllers.Drivers.Services
{
    using global::DeviceManager;
    using global::DeviceManager.Configurations.Device.Driver;
    using System.Threading.Tasks;

    public class DriversControllerService : IDriversControllerService
    {
        private readonly IDeviceManager _useCase;

        public DriversControllerService(IDeviceManager useCase)
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

        public async Task<Driver[]> GetDrivers()
        {
            return await _useCase.GetDrivers();
        }

        public Task<DriverConfiguration> GetDriver(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}