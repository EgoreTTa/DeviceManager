namespace API.Controllers.Drivers
{
    using Core.Configurations.Device.Driver;
    using Core.UseCases;
    using DriverBase.DTOs;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.IO;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DriversController : ControllerBase
    {
        private readonly IDriverUseCase _service;

        public DriversController(IDriverUseCase service) => _service = service;

        [HttpGet]
        public DriverInfo[] GetAll() => _service.GetDriversInfo();

        [HttpPost]
        public async Task UploadDriver(IFormFile uploadedFile)
        {
            await using (var stream = System.IO
                                            .File
                                            .Create(
                                                Path.Combine(Directory.GetCurrentDirectory(), "Drivers", uploadedFile.FileName)))
            {
                await uploadedFile.CopyToAsync(stream);
            }

            _service.Add(new[] { uploadedFile.FileName });
        }

        [HttpDelete("{filename}")]
        public void RemoveDriver(string filename)
        {
            _service.Remove(new[] { filename });
        }
        
        [HttpGet("{name}/options")]
        public OptionDTO[] GetOptionsByNameParser(string name) => _service.GetParser(name).GetOptions();
    }
}