namespace DeviceManagerAPI.Controllers.Drivers
{
    using global::DeviceManager.Configurations.Device.Driver;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Services;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DriversController : ControllerBase
    {
        private readonly IDriversControllerService _service;

        public DriversController(IDriversControllerService driversControllerService) => _service = driversControllerService;

        [HttpGet]
        public async Task<Driver[]> GetAll() => await _service.GetDrivers();

        [HttpPost]
        public async Task<IActionResult> UploadDriver(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                await using var stream = System.IO.File.Create($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}" +
                                                               $"DriversTest{Path.DirectorySeparatorChar}" +
                                                               $"{uploadedFile.FileName}");
                await uploadedFile.CopyToAsync(stream);
            }

            return StatusCode(200);
        }

        [HttpGet("{name}")]
        public FormDriver GetByName(string name)
        {
            // return _service.Find(x => x.Name == name);
            throw new NotImplementedException();
        }
    }
}