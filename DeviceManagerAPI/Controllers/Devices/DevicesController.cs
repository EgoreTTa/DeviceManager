namespace DeviceManagerAPI.Controllers.Devices
{
    using DataAccess.DTOs;
    using Forms;
    using global::DeviceManager;
    using global::DeviceManager.Configurations.Device;
    using global::DeviceManager.Entities;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceManager _service;

        public DevicesController(IDeviceManager service) => _service = service;

        [HttpGet]
        public async Task<DeviceConfiguration[]> GetAll() => await _service.GetDevices();

        [HttpGet("{id}")]
        public async Task<DeviceConfiguration> GetById(int id) => await _service.GetDevice(id);

        [HttpPut]
        public async Task<DeviceManagerEvent> Create(DeviceConfiguration device) => await _service.AddDevice(device);

        [HttpPost("{id}")]
        public async Task<DeviceManagerEvent> Update(int id, DeviceConfiguration device) => await _service.UpdateDevice(id, device);

        [HttpDelete("{id}")]
        public async Task<DeviceManagerEvent> Delete(int id) => await _service.RemoveDevice(id);

        [HttpPut("{id}/flipactive/")]
        public async Task<DeviceManagerEvent> FlipActive(int id) => await _service.FlipActive(id);

        [HttpGet("{id}/comparisons/test-collations")]
        public async Task<TestCollationDto[]> GetTestCollations(int id) => await _service.GetTestCollationsByDeviceId(id);

        [HttpGet("{id}/test-results/")]
        public async Task<TestResult[]> GetTestResults(int id) => await _service.GetTestResultsByDeviceId(id);

        [HttpGet("status/")]
        public async Task<FormStatus[]> Status()
        {
             var devices = await _service.GetDevices();

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