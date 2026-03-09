namespace DeviceManager
{
    using Configurations;
    using Configurations.Device;
    using Configurations.Device.Connection;
    using Configurations.Device.Driver;
    using DataAccess;
    using DataAccess.DTOs;
    using DriverBase;
    using Entities;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DeviceManager : IDeviceManager
    {
        private readonly string _pathDrivers;
        private readonly string _pathDeviceConfigs;
        private readonly string _pathConfig;

        private readonly List<DeviceConfiguration> _deviceConfigurations = new List<DeviceConfiguration>()
        {
            new DeviceConfiguration()
            {
                Id = 1,
                Name = "DeviceName1",
                SystemName = "DeviceSystemName1",
                DriverConfiguration = new DriverConfiguration()
                {
                    Driver = new Driver()
                    {
                        FileName = "DriverTest",
                        Parser = "DriverTest.ASTM"
                    },
                    SystemName = "DriverSystemName1",
                    Encoding = "utf-8"
                },
                ConnectionConfiguration = new ConnectionConfiguration()
                {
                    ConnectionType = ConnectionTypes.Network,
                    Network = new NetworkConnection()
                    {
                        Mode = NetworkModes.Server,
                        Address = "127.0.0.1",
                        Port = 5050
                    },
                    Serial = new SerialConnection()
                    {
                        PortName = "COM1",
                        BaudRate = 9600,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                    },
                    FileSystem = new FileSystemConnection()
                    {
                        FolderToRead = "",
                        FolderToWrite = ""
                    }
                }
            }
        };
        private readonly List<Device> _devices = new List<Device>();
        private readonly List<Type> _types = new List<Type>();
        private readonly List<Driver> _drivers = new List<Driver>();

        private readonly List<(Device device, Task task, CancellationTokenSource source)> _trackings = new List<(Device device, Task task, CancellationTokenSource source)>();

        private readonly AppDbContext _db;
        private DeviceManagerConfiguration _deviceManagerConfiguration;
        private ILogger _logger;

        public DeviceManager()
        {
            _pathConfig = Path.GetFullPath(Path.Combine("Configs"));
            Directory.CreateDirectory(_pathConfig);
            _pathDrivers = Path.GetFullPath(Path.Combine("Drivers"));
            Directory.CreateDirectory(_pathDrivers);
            _pathDeviceConfigs = Path.GetFullPath(Path.Combine($"Configs", "Devices"));
            Directory.CreateDirectory(_pathDeviceConfigs);

            _deviceManagerConfiguration = JsonConvert.DeserializeObject<DeviceManagerConfiguration>(File.ReadAllText(Path.Combine(_pathConfig, "Configuration.json")));
            _logger = new LoggerConfiguration().MinimumLevel.Debug()
                                               .WriteTo.Console()
                                               .WriteTo.File(
                                                   path: Path.Combine("Logs", nameof(DeviceManager), ".txt"),
                                                   rollingInterval: RollingInterval.Day,
                                                   fileSizeLimitBytes: 1024 * 1024,
                                                   rollOnFileSizeLimit: true,
                                                   retainedFileCountLimit: 7,
                                                   shared: true,
                                                   restrictedToMinimumLevel: LogEventLevel.Debug)
                                               .CreateLogger();

            _db = new AppDbContext();
            _db.Database.EnsureCreated();
            _logger.Information("Database EnsureCreated!");

            ReadDeviceConfigurations();
            LoadDrivers(_deviceConfigurations.Select(x => x.DriverConfiguration).ToArray());
        }

        private void ReadDeviceConfigurations()
        {
            var files = Directory.GetFiles(_pathDeviceConfigs, "*.json");
            if (files.Length == 0)
            {
                _logger.Warning("Device configurations not found!");
                foreach (var deviceConfiguration in _deviceConfigurations)
                {
                    File.AppendAllText($"{Path.Combine(_pathDeviceConfigs, deviceConfiguration.Name)}.json",
                        JsonConvert.SerializeObject(deviceConfiguration, Formatting.Indented));

                    _logger.Information("Device configuration template created.");
                }
            }
            else
            {
                _logger.Information($"Device configurations {files.Length} found.");
                _deviceConfigurations.Clear();
                foreach (var file in files)
                {
                    try
                    {
                        _logger.Information($"Device configuration {file} load...");
                        var device = JsonConvert.DeserializeObject<DeviceConfiguration>(File.ReadAllText(file));
                        if (_deviceConfigurations.Any(x => x.Id == device.Id))
                        {
                            device.Id = _deviceConfigurations.Select(x => x.Id).Max() + 1;
                            File.WriteAllText(Path.Combine(_pathDeviceConfigs, file),
                                JsonConvert.SerializeObject(device, Formatting.Indented));
                        }

                        _deviceConfigurations.Add(device);
                        _logger.Information($"Device configuration {file} loaded.");
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, $"Device configuration {file} load is fail! Skip...");
                    }
                }
            }
        }

        private void LoadDrivers(DriverConfiguration[] driverConfigurations)
        {
            var files = driverConfigurations
                        .Where(x => string.IsNullOrEmpty(x.Driver.FileName) is false)
                        .Select(x => $"{Path.Combine(_pathDrivers, x.Driver.FileName)}{(x.Driver.FileName.Contains(".dll") is false ? ".dll" : string.Empty)}")
                        .Distinct();
            foreach (var file in files)
            {
                if (File.Exists($"{file}") is false)
                {
                    _logger.Warning($"Driver {file} not found!");
                    continue;
                }

                _logger.Information($"Driver {file} loading...");

                var driverTypes = Assembly.LoadFrom(file)
                                      .GetTypes()
                                      .Where(type =>
                                          type.IsClass
                                          &&
                                          typeof(IParser).IsAssignableFrom(type));

                foreach (var driverType in driverTypes)
                {
                    _types.Add(driverType);
                    _drivers.Add(new Driver()
                    {
                        FileName = Path.GetFileName(file),
                        Parser = driverType.FullName
                    });
                    _logger.Information($"Driver {driverType.FullName} loaded.");
                }
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            foreach (var deviceConfig in _deviceConfigurations)
            {
                if (deviceConfig.IsActive) await StartDeviceAsync(deviceConfig, token);
            }
        }

        public Task<DeviceConfiguration[]> GetDevices(CancellationToken token = default)
        {
            return Task.FromResult(_deviceConfigurations.ToArray());
        }

        public Task<DeviceConfiguration> GetDevice(int id, CancellationToken token = default)
        {
            return Task.FromResult(_deviceConfigurations.Single(x => x.Id == id));
        }

        public Task<Driver[]> GetDrivers(CancellationToken token = default)
        {
            return Task.FromResult(_drivers.OrderBy(x => x.Parser).ToArray());
        }

        public async Task<DeviceManagerEvent[]> GetEvents(CancellationToken token = default)
        {
            await _db.Events.LoadAsync(token);
            return _db.Events.OrderByDescending(x => x.Id).ToArray();
        }

        public Task<DriverConfiguration> GetDrive(int id, CancellationToken token = default)
        {
            return null;
        }

        public async Task<DeviceManagerEvent> AddDevice(DeviceConfiguration device, CancellationToken token = default)
        {
            await File.AppendAllTextAsync(Path.Combine(_pathDeviceConfigs, $"{device.Id}.json"),
                JsonConvert.SerializeObject(device, Formatting.Indented), token);
            _logger.Information($"Device configuration {device.Name} save.");
            
            _deviceConfigurations.Add(device);

            var newEvent = new DeviceManagerEvent($"Добавлено устройство {device.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync(token);

            return newEvent;
        }

        public async Task<DeviceManagerEvent> RemoveDevice(int id, CancellationToken token = default)
        {
            var device = _deviceConfigurations.Find(x => x.Id == id);
            File.Delete(Path.Combine(_pathDeviceConfigs, $"{device.Id}.json"));
            _logger.Information($"Device configuration {device.Name} deleted!");

            _deviceConfigurations.Remove(device);

            var newEvent = new DeviceManagerEvent($"Удалено устройство {device.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync(token);

            return newEvent;
        }

        public async Task<DeviceManagerEvent> UpdateDevice(int id, DeviceConfiguration beforeDevice, CancellationToken token = default)
        {
            var afterDevice = _deviceConfigurations.Find(x => x.Id == id);
            
            DeviceManagerEvent newEvent;
            if (afterDevice.DriverConfiguration.Driver.Parser != beforeDevice.DriverConfiguration.Driver.Parser)
            {
                newEvent = new DeviceManagerEvent($"Драйвер {beforeDevice.Name} был изменен с {afterDevice.DriverConfiguration.Driver.Parser} на {beforeDevice.DriverConfiguration.Driver.Parser}");
                _db.Events.Add(newEvent);
                await _db.SaveChangesAsync(token);
            
                var parserType = _types.Single(x => x.FullName == beforeDevice.DriverConfiguration.Driver.Parser);
                if (_devices.SingleOrDefault(x => x.Configuration.Id == id) is { } device)
                {
                    device.Parser = Activator.CreateInstance(parserType) as IParser;
                }
            }

            _deviceConfigurations[_deviceConfigurations.IndexOf(afterDevice)] = beforeDevice;
            await File.WriteAllTextAsync(Path.Combine(_pathDeviceConfigs, $"{beforeDevice.Id}.json"),
                JsonConvert.SerializeObject(beforeDevice, Formatting.Indented), token);

            newEvent = new DeviceManagerEvent($"Обновлена конфигурация устройства {beforeDevice.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync(token);

            return newEvent;
        }

        public Task<DeviceManagerConfiguration> GetSettings() => Task.FromResult(_deviceManagerConfiguration);

        public async Task<DeviceManagerEvent> UpdateSettings(DeviceManagerConfiguration deviceManagerConfiguration)
        {
            _deviceManagerConfiguration = deviceManagerConfiguration;

            var newEvent = new DeviceManagerEvent($"Обновлена конфигурация Device Manager!");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return newEvent;
        }

        public async Task<DeviceManagerEvent> FlipActive(int id, CancellationToken token = default)
        {
            var deviceConfig = _deviceConfigurations.Single(x => x.Id == id);
            deviceConfig.IsActive = !deviceConfig.IsActive;
            await UpdateDevice(id, deviceConfig, token);

            var newEvent = new DeviceManagerEvent($"{(deviceConfig.IsActive ? "Включено" : "Отключено")} устройство {deviceConfig.Name}!");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync(token);

            if (deviceConfig.IsActive)
            {
                _logger.Warning($"Включение {deviceConfig.Name}...");
                await StartDeviceAsync(deviceConfig, token);
                _logger.Warning($"Включено {deviceConfig.Name}!");
            }
            else
            {
                _logger.Warning($"Выключение {deviceConfig.Name}...");
                var device = _devices.Single(x => x.Configuration.Id == id);
                await device.StopAsync(token);
                _devices.Remove(device);
                _logger.Warning($"Выключено {deviceConfig.Name}!");
            }

            return newEvent;
        }

        public async Task<TestResult[]> GetTestResultsByDeviceId(int id)
        {
            await _db.TestResults.LoadAsync();
            return _db.TestResults.Where(x => x.DeviceId == id).ToArray();
        }

        public Task<TestCollationDto[]> GetTestCollationsByDeviceId(int id)
        {
            var device = _devices.SingleOrDefault(x => x.Configuration.Id == id);
            return Task.FromResult(device == null
                ? new TestCollationDto[] { }
                : device.TestCollationDto);
        }

        private async Task StartDeviceAsync(DeviceConfiguration configuration, CancellationToken token)
        {
            try
            {
                var logger = new LoggerConfiguration().MinimumLevel.Debug()
                                                      .WriteTo.Console()
                                                      .WriteTo.File(
                                                          path: Path.Combine("Logs", configuration.Name, ".txt"),
                                                          rollingInterval: RollingInterval.Day,
                                                          fileSizeLimitBytes: 1024 * 1024,
                                                          rollOnFileSizeLimit: true,
                                                          retainedFileCountLimit: 7,
                                                          shared: true)
                                                      .CreateLogger();
                var dataAccess = new DataAccess(_deviceManagerConfiguration.Address);

                var parserType = _types.Single(x => x.FullName == configuration.DriverConfiguration.Driver.Parser);

                var parser = Activator.CreateInstance(parserType) as IParser;

                var device = new Device(logger, configuration, parser, dataAccess)
                {
                    DbContext = _db
                };
                await device.StartAsync();
                _logger.Warning($"Запущено {configuration.Name}!");
                _devices.Add(device);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                _logger.Warning($"{configuration.Name} не был запущен!");
            }
        }

        public async Task<DeviceManagerEvent> LoadDriver(string filename)
        {
            var file = Path.Combine(_pathDrivers, filename);
            if (File.Exists($"{file}") is false)
            {
                _logger.Warning($"Driver {filename} not found!");
            }

            _logger.Information($"Driver {filename} loading...");
            DeviceManagerEvent newEvent;
            try
            {
                var driverTypes = Assembly.LoadFrom(file)
                                          .GetTypes()
                                          .Where(type =>
                                              type.IsClass
                                              &&
                                              typeof(IParser).IsAssignableFrom(type));

                foreach (var driverType in driverTypes)
                {
                    _types.Add(driverType);
                    _drivers.Add(new Driver()
                    {
                        FileName = filename,
                        Parser = driverType.FullName
                    });
                    _logger.Information($"Driver {driverType.FullName} loaded.");
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                _logger.Fatal(exception.StackTrace);
                newEvent = new DeviceManagerEvent($"Ошибка загрузки {filename}.");
                _db.Events.Add(newEvent);
                await _db.SaveChangesAsync();

                return newEvent;
            }

            newEvent = new DeviceManagerEvent($"Загружен драйвер {filename}.");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return newEvent;
        }
    }
}