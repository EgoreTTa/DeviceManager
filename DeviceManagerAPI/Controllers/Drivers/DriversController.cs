using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeviceManagerAPI.Controllers.Drivers
{
    [ApiController]
    [Route("[controller]/")]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;

        private static readonly List<FormDriver> _drivers = new List<FormDriver>()
        {
            new FormDriver()
            {
                FileName = "Arhitect.dll",
                Name = "ASTM F2312-11",
                Version = "1.3",
                DateTimeBuild = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                WorkModes = new[] { "1", "2", "3" },
            },
            new FormDriver()
            {
                FileName = "BC5300.dll",
                Name = "HL7 v2.1",
                Version = "0.2",
                DateTimeBuild = $"{DateTime.Now - TimeSpan.FromSeconds(new Random().Next(604800)):yyyy-MM-dd HH:mm:ss}",
                WorkModes = new[] { "1", "2" },
            },
            new FormDriver()
            {
                FileName = "Minicap.dll",
                Name = "Custom",
                Version = "1.0",
                DateTimeBuild = $"{DateTime.Now - TimeSpan.FromSeconds(new Random().Next(604800)):yyyy-MM-dd HH:mm:ss}",
                WorkModes = new[] { "1" },
            },
        };

        public DriversController(ILogger<DriversController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public FormDriver[] GetAll()
        {
            return _drivers.ToArray();
        }

        [HttpGet("{name}")]
        public FormDriver GetByName(string name)
        {
            return _drivers.Find(x => x.Name == name);
        }
    }
}