using DataAccess.DTOs;

namespace DeviceManagerAPI.Controllers.Devices
{
    using Forms;
    using global::DeviceManager.Configurations.Device;
    using global::DeviceManager.Entities;
    using Microsoft.AspNetCore.Mvc;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DevicesController : ControllerBase
    {
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
        public async Task<DeviceManagerEvent> Create(DeviceConfiguration device)
        {
            return await _devicesControllerService.AddDevice(device);
        }

        [HttpPost("{id}")]
        public async Task<DeviceManagerEvent> Update(int id, DeviceConfiguration device)
        {
            return await _devicesControllerService.UpdateDevice(id, device);
        }

        [HttpDelete("{id}")]
        public async Task<DeviceManagerEvent> Delete(int id)
        {
            return await _devicesControllerService.RemoveDevice(id);
        }

        [HttpPut("{id}/flipactive/")]
        public async Task<DeviceManagerEvent> FlipActive(int id)
        {
            return await _devicesControllerService.FlipActive(id);
        }

        [HttpGet("{id}/getcomparisons/")]
        public IActionResult GetComparisons(int id)
        {
            return StatusCode(200);
        }

        [HttpGet("{id}/testresults/")]
        public async Task<TestResult[]> GetTestResults(int id)
        {
            return await _devicesControllerService.GetTestResultsByDeviceId(id);
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