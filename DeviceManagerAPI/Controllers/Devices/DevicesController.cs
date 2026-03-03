using System.Threading.Tasks;

namespace DeviceManagerAPI.Controllers.Devices
{
    using Forms;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    [ApiController]
    [Route("[controller]/")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IDevicesControllerService _devicesControllerService;

        private static readonly List<DeviceForm> _devices = new List<DeviceForm>()
        {
            new DeviceForm()
            {
                Id = 1,
                Name = "Device-Name-1",
                SystemName = "Device-SystemName-1",
                Driver = new FormDeviceDriver()
                {
                    Name = "DeviceDriver-Name-1",
                    SystemName = "DeviceDriver-SystemName-1",
                    Encoding = "ascii",
                    WorkMode = "WorkMode-1",
                    AddressType = "AddressType-1",
                    Options = new string[] { },
                    ExtraOptions = ""
                },
                Hardware = new FormHardware()
                {
                    Type = "TcpIp",
                    TcpIp = new FormHardware.TcpIpConnection()
                    {
                        Mode = "Server",
                        Host = "127.0.0.1",
                        Port = 5000
                    }
                },
                IsActive = true
            }
        };

        public DevicesController(IDevicesControllerService devicesControllerService)
        {
            _devicesControllerService = devicesControllerService;
        }

        [HttpGet]
        public async Task<DeviceForm[]> GetAll()
        {
            return await _devicesControllerService.GetDevices();
            // return _devices.ToArray();
        }

        [HttpGet("{id}")]
        public DeviceForm GetById(int id)
        {
            return _devices.Find(x => x.Id == id);
        }

        [HttpPut]
        public async Task<IActionResult> Create(DeviceForm deviceForm)
        {
            // deviceForm.Id = _devices.Max(x => x.Id) + 1;
            // _devices.Add(deviceForm);

            // var newId = _devicesControllerService.GetDevices().Max() + 1;
            try
            {
                await _devicesControllerService.AddDevice(deviceForm);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now}\t" + exception.Message);
                Console.WriteLine($"{DateTime.Now}\t" + exception.StackTrace);

            }


            return StatusCode(201);
        }

        [HttpPost("{id}")]
        public IActionResult Update(int id, DeviceForm after)
        {
            var deviceIndex = _devices.IndexOf(_devices.Find(x => x.Id == id));
            _devices[deviceIndex] = after;
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
            var device = _devices.Single(x => x.Id == id);
            device.IsActive = !device.IsActive;
            return StatusCode(200);
        }

        [HttpGet("{id}/getcomparisons/")]
        public IActionResult GetComparisons(int id)
        {
            return StatusCode(200);
        }

        [HttpGet("status/")]
        public FormStatus[] Status()
        {
            var devicesStatus = new List<FormStatus>();

            foreach (var device in _devices)
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