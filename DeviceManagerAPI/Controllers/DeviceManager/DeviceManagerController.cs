using System;
using System.IO;
using System.Linq;
using DeviceManagerAPI.Controllers.Devices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeviceManagerAPI.Controllers.DeviceManager
{
    [ApiController]
    [Route("[controller]/")]
    public class DeviceManagerController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private static FormDeviceManagerSettings _formDeviceManagerSettings = new FormDeviceManagerSettings()
        {
            Address = "http://mis/",
            TimeOut = "15 sec",
            IsLogRequest = false
        };

        public DeviceManagerController(ILogger<DevicesController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet(nameof(GetLogs))]
        public string GetLogs()
        {
            return "get logs";
        }

        [HttpGet(nameof(GetDevices))]
        public string[] GetDevices()
        {
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Drivers");
            var files = Directory.CreateDirectory(uploadsDir).GetFiles();
            return files.Select(x => x.Name.Split('.').First()).ToArray();
        }

        [HttpPost(nameof(UploadDriver))]
        public IActionResult UploadDriver(IFormFile file)
        {
            // Проверка: файл передан?
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран.");

            // Опционально: проверка типа/размера
            if (file.Length > 40 * 1024 * 1024) // 40 MB
                return BadRequest("Файл больше 40MB.");
            if (file.Length < 5 * 1024 * 1024) // 5 MB
                return BadRequest("Файл меньше 5MB.");

            var allowedType = ".dll";
            if (file.ContentDisposition.Contains(allowedType) is false)
                return BadRequest("Недопустимый тип файла.");

            // Сохранение (пример — в папку wwwroot/uploads)
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Drivers");
            Directory.CreateDirectory(uploadsDir); // создать, если нет
            var filePath = Path.Combine(uploadsDir, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Возврат успешного результата
            return CreatedAtAction(nameof(UploadDriver), new { fileName = file.FileName });
            // или return Ok(new { Url = $"/uploads/{file.FileName}" });
        }

        [HttpGet(nameof(GetSettings))]
        public FormDeviceManagerSettings GetSettings()
        {
            return _formDeviceManagerSettings;
        }

        [HttpGet("errors")]
        public FormDeviceManagerError[] GetErrors()
        {
            var errors = new FormDeviceManagerError[new Random().Next(10)];

            for (var i = 0; i < errors.Length; i++)
            {
                errors[i] = new FormDeviceManagerError
                {
                    DateTime = $"{DateTime.Now - TimeSpan.FromSeconds(new Random().Next(604800)):yyyy-MM-dd HH:mm:ss}",
                    Error = new Random().Next(5) switch
                    {
                        0 => "Нет доступа до ЛИС...",
                        1 => "Возникла неконтролируемая ошибка в устройстве {device.name} с драйвером {driver.name}. Попытка перезапустить устройство.",
                        2 => "Нет доступа до порта {port.name} при запуске устройства {device.name}.",
                        3 => "Пропала связь с ЛА в работе устройства {device.name}.",
                        _ => "Ошибка другого типа...",
                    }
                };
            }

            return errors;
        }

        [HttpPost(nameof(UpdateSettings))]
        public IActionResult UpdateSettings(FormDeviceManagerSettings formDeviceManagerSettings)
        {
            _formDeviceManagerSettings = formDeviceManagerSettings;
            return StatusCode(200);
        }
    }
}