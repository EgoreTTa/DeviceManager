namespace DeviceManagerAPI.Controllers.Devices
{
    using DeviceManagerService.Configurations.Device;
    using Forms;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IDevicesControllerService _devicesControllerService;

        public DevicesController(IDevicesControllerService devicesControllerService)
        {
            _devicesControllerService = devicesControllerService;
        }

        [HttpGet]
        public async Task<DeviceConfiguration[]> GetAll()
        {
            return await _devicesControllerService.GetDevices();
        }

        [HttpGet("{id}")]
        public async Task<DeviceConfiguration> GetById(int id)
        {
            return await _devicesControllerService.GetDevice(id);
        }

        [HttpPut]
        public async Task<IActionResult> Create(DeviceConfiguration device)
        {
            await _devicesControllerService.AddDevice(device);

            return StatusCode(201);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Update(int id, DeviceConfiguration device)
        {
            await _devicesControllerService.UpdateDevice(id, device);

            return StatusCode(200);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // _devices.Remove(_devices.Find(x => x.Id == id));

            await _devicesControllerService.RemoveDevice(id);

            return StatusCode(200);
        }

        [HttpPut("{id}/flipactive/")]
        public IActionResult FlipActive(int id)
        {
            // var device = _devices.Single(x => x.Id == id);
            // device.IsActive = !device.IsActive;
            return StatusCode(200);
        }

        [HttpGet("{id}/getcomparisons/")]
        public IActionResult GetComparisons(int id)
        {
            return StatusCode(200);
        }

        [HttpGet("status/")]
        public async Task<FormStatus[]> Status()
        {
             var devices = await _devicesControllerService.GetDevices();

             var devicesStatus = new List<FormStatus>();
             foreach (var device in devices)
             {
                 var countMessagesQuery = new Random().Next(100);
                 var countMessagesOrder = new Random().Next(countMessagesQuery);
                 var countMessagesResult = countMessagesOrder;
                 var countMessagesError = countMessagesQuery - countMessagesOrder;

                 devicesStatus.Add(new FormStatus()
                 {
                     DeviceName = device.Name,
                     CountMessagesQuery = $"{countMessagesQuery}",
                     CountMessagesOrder = $"{countMessagesOrder}",
                     CountMessagesResult = $"{countMessagesResult}",
                     CountMessagesError = $"{countMessagesError}"
                 });
             }

             return devicesStatus.ToArray();
        }
    }
}