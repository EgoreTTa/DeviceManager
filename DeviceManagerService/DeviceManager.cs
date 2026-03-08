using System.Security.Cryptography.X509Certificates;
using DataAccess.DTOs;

namespace DeviceManager
{
    using Configurations;
    using Configurations.Device;
    using Configurations.Device.Connection;
    using Configurations.Device.Driver;
    using DataAccess;
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

        private readonly List<DeviceConfiguration> _devices = new List<DeviceConfiguration>()
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
        private readonly List<(Type type, IParser parser)> _parsers = new List<(Type type, IParser parser)>();
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
            LoadDrivers(_devices.Select(x => x.DriverConfiguration).ToArray());
        }

        private void ReadDeviceConfigurations()
        {
            var files = Directory.GetFiles(_pathDeviceConfigs, "*.json");
            if (files.Length == 0)
            {
                _logger.Warning("Device configurations not found!");
                foreach (var deviceConfiguration in _devices)
                {
                    File.AppendAllText($"{Path.Combine(_pathDeviceConfigs, deviceConfiguration.Name)}.json",
                        JsonConvert.SerializeObject(deviceConfiguration, Formatting.Indented));

                    _logger.Information("Device configuration template created.");
                }
            }
            else
            {
                _logger.Information($"Device configurations {files.Length} found.");
                _devices.Clear();
                foreach (var file in files)
                {
                    try
                    {
                        _logger.Information($"Device configuration {file} load...");
                        var device = JsonConvert.DeserializeObject<DeviceConfiguration>(File.ReadAllText(file));
                        if (_devices.Any(x => x.Id == device.Id))
                        {
                            device.Id = _devices.Select(x => x.Id).Max() + 1;
                            File.WriteAllText(Path.Combine(_pathDeviceConfigs, file),
                                JsonConvert.SerializeObject(device, Formatting.Indented));
                        }

                        _devices.Add(device);
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
                                          typeof(IParser).IsAssignableFrom(type)
                                      );
                foreach (var driverType in driverTypes)
                {
                    var type = Activator.CreateInstance(driverType);
                    if (type == null) continue;

                    _parsers.Add((type.GetType(), type as IParser));
                    _drivers.Add(new Driver()
                    {
                        FileName = file,
                        Parser = driverType.FullName
                    });
                    _logger.Information($"Driver {file} loaded.");
                }
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            // while (token.IsCancellationRequested is false)
            // {
            //     try
            //     {
            //         _logger.Information($"Tracking start...");
            //
            //
            //
            //         _logger.Information($"Tracking end.");
            //     }
            //     catch (Exception exception)
            //     {
            //         _logger.Error(exception.Message);
            //     }
            //     finally
            //     {
            //         await Task.Delay(TimeSpan.FromSeconds(1), token);
            //     }
            // }
        }

        public Task<DeviceConfiguration[]> GetDevices(CancellationToken token = default)
        {
            return Task.FromResult(_devices.ToArray());
        }

        public Task<DeviceConfiguration> GetDevice(int id, CancellationToken token = default)
        {
            return Task.FromResult(_devices.Single(x => x.Id == id));
        }

        public Task<Driver[]> GetDrivers(CancellationToken token = default)
        {
            return Task.FromResult(_drivers.ToArray());
        }

        public async Task<DeviceManagerEvent[]> GetEvents(CancellationToken token = default)
        {
            await _db.Events.LoadAsync(token);
            return _db.Events.ToArray();
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
            
            _devices.Add(device);

            var newEvent = new DeviceManagerEvent($"Добавлено устройство {device.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync(token);

            return newEvent;
        }

        public async Task<DeviceManagerEvent> RemoveDevice(int id, CancellationToken token = default)
        {
            var device = _devices.Find(x => x.Id == id);
            File.Delete(Path.Combine(_pathDeviceConfigs, $"{device.Id}.json"));
            _logger.Information($"Device configuration {device.Name} deleted!");

            _devices.Remove(device);

            var newEvent = new DeviceManagerEvent($"Удалено устройство {device.Name}");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync(token);

            return newEvent;
        }

        public async Task<DeviceManagerEvent> UpdateDevice(int id, DeviceConfiguration device, CancellationToken token = default)
        {
            _devices[_devices.IndexOf(_devices.Find(x => x.Id == id))] = device;

            await File.WriteAllTextAsync(Path.Combine(_pathDeviceConfigs, $"{device.Id}.json"),
                JsonConvert.SerializeObject(device, Formatting.Indented), token);

            var newEvent = new DeviceManagerEvent($"Обновлена конфигурация устройства {device.Name}");
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

        public async Task<DeviceManagerEvent> FlipActive(int id, CancellationToken token)
        {
            var device = _devices[_devices.IndexOf(_devices.Find(x => x.Id == id))];
            device.IsActive = !device.IsActive;
            _devices[_devices.IndexOf(_devices.Find(x => x.Id == id))] = device;

            var newEvent = new DeviceManagerEvent($"{(device.IsActive ? "Включено" : "Отключено")} устройство {device.Name}!");
            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync(token);

            if (device.IsActive) await StartDeviceAsync(device, token);
            else
            {
                var track = _trackings.Single(x => x.device.Configuration.Id == id);
                track.source.Cancel();
                await Task.WhenAny(track.task, Task.Delay(TimeSpan.FromSeconds(5), token));
                track.source.Dispose();
            }



            return newEvent;
        }

        public async Task<TestResult[]> GetTestResultsByDeviceId(int id)
        {
            await _db.TestResults.LoadAsync();
            return _db.TestResults.Where(x => x.DeviceId == id).ToArray();
        }

        private async Task StartDeviceAsync(DeviceConfiguration configuration, CancellationToken token)
        {
            var device = new Device()
            {
                Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                                  .WriteTo.Console()
                                                  .WriteTo.File(
                                                      path: Path.Combine("Logs", configuration.Name, ".txt"),
                                                      rollingInterval: RollingInterval.Day,
                                                      fileSizeLimitBytes: 1024 * 1024,
                                                      rollOnFileSizeLimit: true,
                                                      retainedFileCountLimit: 7,
                                                      shared: true)
                                                  .CreateLogger(),
                Configuration = configuration
            };

            var parserType = Type.GetType(configuration.DriverConfiguration.Driver.Parser);
            if (parserType == null ) return;

            var parser = Activator.CreateInstance(parserType) as IParser;
            if (parser == null) return;
            parser.Logger = device.Logger;

            var dataAccess = new DataAccess(_deviceManagerConfiguration.Address,
                configuration.SystemName,
                configuration.DriverConfiguration.SystemName);

            var source = new CancellationTokenSource();
            _trackings.Add((device, device.StartAsync(dataAccess, parser, source.Token), source));
        }
    }
}