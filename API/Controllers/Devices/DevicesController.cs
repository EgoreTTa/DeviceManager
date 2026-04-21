namespace API.Controllers.Devices
{
    using Core.Configurations.Device;
    using Core.Entities;
    using Core.UseCases;
    using DriverBase.DTOs;
    using Infrastructure.DTOs.LIS;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceUseCase _service;

        public DevicesController(IDeviceUseCase service) => _service = service;

        [HttpGet]
        public DeviceConfig[] GetAll()
        {
            // Console.WriteLine($"Devices get all");
            return _service.GetDevices()
                           .Select(device => device.Config)
                           .ToArray();
        }

        [HttpGet("{id}")]
        public DeviceConfig GetDeviceById(int id) => _service.GetDevices()
                                                             .Select(device => device.Config)
                                                             .Single(config => config.Id == id);

        [HttpPut]
        public async Task<DeviceManagerEvent> Create(DeviceConfig device) => await _service.AddDevice(device);

        [HttpPost("{id}")]
        public async Task<DeviceManagerEvent> Update(int id, DeviceConfig device) => await _service.UpdateDevice(id, device);

        [HttpDelete("{id}")]
        public async Task<DeviceManagerEvent> Delete(int id) => await _service.RemoveDevice(id);

        [HttpPut("{id}/flip-active")]
        public async Task<DeviceManagerEvent> FlipActive(int id)
        {
            var device = _service.GetDevices().Single(x => x.Config.Id == id);
            if (device.Config.IsActive)
            {
                await device.StopAsync();
                device.Config.IsActive = false;
            }
            else
            {
                await device.StartAsync();
                device.Config.IsActive = true;
            }
            await _service.UpdateDevice(id, device.Config);

            return null;
        }

        [HttpGet("{id}/comparisons/enum-values")]
        public EnumValueDto[] GetEnumValues(int id)
        {
            var device = _service.GetDevices().Single(x => x.Config.Id == id);
            return device.DeviceService.EnumValueDtos;
        }

        [HttpGet("{id}/comparisons/measure-units")]
        public MeasureUnitDto[] GetMeasureUnits(int id)
        {
            var device = _service.GetDevices().Single(x => x.Config.Id == id);
            return device.DeviceService.MeasureUnitDtos;
        }

        [HttpGet("{id}/comparisons/test-collations")]
        public TestCollationDto[] GetTestCollations(int id)
        {
            var device = _service.GetDevices().Single(x => x.Config.Id == id);
            return device.DeviceService.TestCollationDtos;
        }

        [HttpGet("{id}/test-results")]
        public async Task<TestResultDTO[]> GetTestResults(int id)
        {
            return await _service.GetTestResults(id);
        }

        [HttpPost("{id}/test-results/{testResultId}")]
        public async Task<DeviceManagerEvent> RetrySendTestResult(int id, int testResultId) => await _service.RetrySendTestResult(id, testResultId);

        [HttpGet("{id}/logs")]
        public string[] GetLogs(int id)
        {
            var device = _service.GetDevices().Single(x => x.Config.Id == id);
            return device.DeviceLogs.Logs;
        }

        [HttpGet("{id}/driver/options")]
        public OptionDTO[] GetDriverOptions(int id)
        {
            var device = _service.GetDevices().Single(x => x.Config.Id == id);
            return device.Parser.GetOptions();
        }

        [HttpPost("{id}/driver/options")]
        public void SetDriverOptions(int id, OptionDTO[] options)
        {
            var device = _service.GetDevices().Single(x => x.Config.Id == id);
            device.Parser.SetOptions(options);
        }
    }
}