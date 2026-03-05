namespace DeviceManagerAPI.Controllers.Drivers
{
    using DeviceManagerService.Configurations.Device.Driver;
    using Microsoft.AspNetCore.Mvc;
    using Services;
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DriversController : ControllerBase
    {
        private readonly IDriversControllerService _service;

        public DriversController(IDriversControllerService driversControllerService)
        {
            _service = driversControllerService;
        }

        [HttpGet]
        public async Task<DriverConfiguration[]> GetAll()
        {
            return await _service.GetDrivers();
        }

        [HttpGet("{name}")]
        public FormDriver GetByName(string name)
        {
            // return _service.Find(x => x.Name == name);
            throw new NotImplementedException();
        }
    }
}