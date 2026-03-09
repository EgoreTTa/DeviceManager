namespace DeviceManagerAPI.Controllers.Drivers
{
    using global::DeviceManager;
    using global::DeviceManager.Configurations.Device.Driver;
    using global::DeviceManager.Entities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DriversController : ControllerBase
    {
        private readonly IDeviceManager _service;

        public DriversController(IDeviceManager service) => _service = service;

        [HttpGet]
        public async Task<Driver[]> GetAll() => await _service.GetDrivers();

        [HttpPost]
        public async Task<DeviceManagerEvent> UploadDriver(IFormFile uploadedFile)
        {
            await using (var stream = System.IO.File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Drivers", uploadedFile.FileName)))
            {
                await uploadedFile.CopyToAsync(stream);
            }

            return await _service.LoadDriver(uploadedFile.FileName);
        }

        [HttpGet("{name}")]
        public Task<IActionResult> GetByName(string name)
        {
            // return _service.Find(x => x.Name == name);
            throw new NotImplementedException();
        }
    }
}