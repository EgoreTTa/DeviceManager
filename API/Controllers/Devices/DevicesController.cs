namespace API.Controllers.Devices
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [ApiController]
    [Route("[controller]/")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private static readonly List<FormDevice> _devices = new List<FormDevice>()
        {
            new FormDevice()
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

        public DevicesController(ILogger<DevicesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public FormDevice[] GetAll()
        {
            return _devices.ToArray();
        }

        [HttpGet("{id}")]
        public FormDevice GetById(int id)
        {
            return _devices.Find(x => x.Id == id);
        }

        [HttpPut]
        public IActionResult Create(FormDevice formDevice)
        {
            formDevice.Id = _devices.Max(x => x.Id) + 1;
            _devices.Add(formDevice);
            return StatusCode(201);
        }

        [HttpPost("{id}")]
        public IActionResult Update(int id, FormDevice afterDevice)
        {
            var deviceIndex = _devices.IndexOf(_devices.Find(x => x.Id == id));
            _devices[deviceIndex] = afterDevice;
            return StatusCode(200);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _devices.Remove(_devices.Find(x => x.Id == id));
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