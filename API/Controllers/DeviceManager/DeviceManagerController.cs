namespace API.Controllers.DeviceManager
{
    using Core.Configurations;
    using Core.Entities;
    using Core.UseCases;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]/")]
    public class DeviceManagerController : ControllerBase
    {
        private readonly IDeviceUseCase _service;

        public DeviceManagerController(IDeviceUseCase service) => _service = service;

        [HttpGet(nameof(GetSettings))]
        public DeviceManagerConfig GetSettings() => _service.GetSettings();

        // [HttpGet("errors")]
        // public FormDeviceManagerError[] GetErrors()
        // {
        //     var errors = new FormDeviceManagerError[new Random().Next(10)];
        //
        //     for (var i = 0; i < errors.Length; i++)
        //     {
        //         errors[i] = new FormDeviceManagerError
        //         {
        //             DateTime = $"{DateTime.Now - TimeSpan.FromSeconds(new Random().Next(60480 * i)):yyyy-MM-dd HH:mm:ss}",
        //             Error = new Random().Next(16) switch
        //             {
        //                 0 => "Нет доступа до ЛИС...",
        //                 1 => "Возникла неконтролируемая ошибка в устройстве {device.name} с драйвером {driver.name}. Попытка перезапустить устройство.",
        //                 2 => "Нет доступа до порта {port.name} при запуске устройства {device.name}.",
        //                 3 => "Пропала связь с ЛА в работе устройства {device.name}.",
        //                 4 => "Отключено устройство {device.name}",
        //                 5 => "Включено устройство {device.name}",
        //                 6 => "Удалено устройство {device.name}",
        //                 7 => "Добавлено устройство {device.name}",
        //                 8 => "Загружен драйвер {driver.name}",
        //                 9 => "Не найден драйвер {driver.name} для запуска устройства, устройство {device.name} отключено...",
        //                 10 => "Былы отключено устройство {device.name}",
        //                 11 => "Изменены конфигурации приложения!",
        //                 12 => "Изменен способ подключения устройства {device.name} с {device.connection.type} на {device.connection.type}",
        //                 13 => "Изменено системное имя устройства {device.name}",
        //                 14 => "Были изменены файлы конфигураций извне, возможны сбои!",
        //                 _ => "ещё не добавленный тип событий...",
        //             }
        //         };
        //     }
        //
        //     return errors;
        // }

        [HttpGet("journal")]
        public async Task<DeviceManagerEvent[]> GetEvents() => await _service.GetEvents();

        [HttpPut(nameof(UpdateSettings))]
        public async Task<DeviceManagerEvent> UpdateSettings(DeviceManagerConfig configuration) => await _service.UpdateSettings(configuration);
    }
}