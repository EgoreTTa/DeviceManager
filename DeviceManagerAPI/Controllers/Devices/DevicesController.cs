namespace DeviceManagerAPI.Controllers.Devices
{
    using DataAccess.DTOs.LIS;
    using DriverBase.DTOs;
    using global::DeviceManager.Configurations.Device;
    using global::DeviceManager.Entities;
    using global::DeviceManager.UseCases;
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
        public DeviceConfig[] GetAll() => _service.GetDevices()
                                                  .Select(device => device.Configuration)
                                                  .ToArray();

        [HttpPut]
        public async Task<DeviceManagerEvent> Create(DeviceConfig device) => await _service.AddDevice(device);

        [HttpPost("{id}")]
        public async Task<DeviceManagerEvent> Update(int id, DeviceConfig device) => await _service.UpdateDevice(id, device);

        [HttpDelete("{id}")]
        public async Task<DeviceManagerEvent> Delete(int id) => await _service.RemoveDevice(id);

        [HttpPut("{id}/flip-active")]
        public async Task<DeviceManagerEvent> FlipActive(int id)
        {
            var device = _service.GetDevices().Single(x => x.Configuration.Id == id);
            if (device.Configuration.IsActive)
            {
                await device.StopAsync();
                device.Configuration.IsActive = false;
            }
            else
            {
                await device.StartAsync();
                device.Configuration.IsActive = true;
            }

            return null;
        }

        [HttpGet("{id}/comparisons/test-collations")]
        public TestCollationDto[] GetTestCollations(int id)
        {
            var device = _service.GetDevices().Single(x => x.Configuration.Id == id);
            return device.GetTestCollations();
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
            var device = _service.GetDevices().Single(x => x.Configuration.Id == id);
            return device.DeviceLogs.Logs;
        }

        [HttpGet("{id}/driver/options")]
        public OptionDTO[] GetDriverOptions(int id)
        {
            var device = _service.GetDevices().Single(x => x.Configuration.Id == id);
            return device.Parser.GetOptions();
        }

        [HttpPost("{id}/driver/options")]
        public void SetDriverOptions(int id, OptionDTO[] options)
        {
            var device = _service.GetDevices().Single(x => x.Configuration.Id == id);
            device.Parser.SetOptions(options);
        }
    }
}