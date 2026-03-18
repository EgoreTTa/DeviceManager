namespace Core.UseCases
{
    using Configurations;
    using Configurations.Device;
    using Devices;
    using DriverBase.DTOs;
    using Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using UseCaseServices;

    public sealed class DeviceUseCase : IDeviceUseCase
    {
        private readonly IDriverUseCaseService _driverUCS;
        private readonly IDeviceUseCaseService _deviceUCS;
        private readonly AppDbContext _db; // todo service

        public DeviceUseCase(
            IDriverUseCaseService driverUcs,
            IDeviceUseCaseService deviceUcs)
        {
            _driverUCS = driverUcs;
            _deviceUCS = deviceUcs;

            _db = new AppDbContext();
            _db.Database.EnsureCreated();
        }

        public Device[] GetDevices() => _deviceUCS.GetDevices();

        public async Task ReadDeviceConfigs()
        {
            await _deviceUCS.ReadDeviceConfigs(_db);
            var devices = _deviceUCS.GetDevices();

            _db.Events.Add(new DeviceManagerEvent($"Загрузка конфигураций...")
            {
                Details = $"Загружено {devices.Length} конфигураций."
            });
            await _db.SaveChangesAsync();
        }

        public async Task<DeviceManagerEvent[]> GetEvents()
        {
            await _db.Events.LoadAsync();
            return _db.Events.ToArray()
                      .TakeLast(100)
                      .OrderByDescending(x => x.Id)
                      .ToArray();
        }

        public async Task<DeviceManagerEvent> AddDevice(DeviceConfig device)
        {
            var newDevice = await _deviceUCS.AddDevice(device, _db);
            var parser = _driverUCS.GetParser(device.Driver.Parser.FullName);

            newDevice.Parser = parser;
            newDevice.Parser.SetOptions(device.Driver.Parser.Options);

            var newEvent = new DeviceManagerEvent($"Добавлено устройство {device.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return newEvent;
        }

        public async Task<DeviceManagerEvent> RemoveDevice(int id)
        {
            var device = _deviceUCS.GetDevices().Single(x => x.Configuration.Id == id);
            await _deviceUCS.RemoveDevice(id);

            var newEvent = new DeviceManagerEvent($"Удалено устройство {device.Configuration.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return newEvent;
        }

        public async Task<DeviceManagerEvent> UpdateDevice(int id, DeviceConfig afterDevice)
        {
            await _deviceUCS.UpdateDevice(id, afterDevice);
            var newEvent = new DeviceManagerEvent($"Обновлена конфигурация устройства {afterDevice.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return newEvent;
        }

        public DeviceManagerConfig GetSettings() => _deviceUCS.GetSettings();

        public async Task<DeviceManagerEvent> UpdateSettings(DeviceManagerConfig config)
        {
            await _deviceUCS.UpdateSettings(config);
            var newEvent = new DeviceManagerEvent($"Обновлена конфигурация Device Manager!");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return newEvent;
        }

        public async Task<DeviceManagerEvent> RetrySendTestResult(int id, int testResultId)
        {
            var device = _deviceUCS.GetDevices().Single(x => x.Configuration.Id == id);
            await _deviceUCS.RetrySendTestResult(id, testResultId);

            var newEvent = new DeviceManagerEvent($"Ручная отправка данных c {device.Configuration.Name} в ЛИС");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return newEvent;
        }

        public async Task<TestResultDTO[]> GetTestResults(int id)
        {
            var device = _deviceUCS.GetDevices().Single(x => x.Configuration.Id == id);
            await _db.TestResults.LoadAsync();

            return _db.TestResults.Where(x => x.DeviceSystemName == device.Configuration.SystemName).ToArray();
        }
    }
}