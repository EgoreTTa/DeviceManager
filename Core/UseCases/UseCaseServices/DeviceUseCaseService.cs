namespace Core.UseCases.UseCaseServices
{
    using Configurations;
    using Configurations.Device;
    using Configurations.Device.Connection;
    using Devices.Components.Connect;
    using Devices.Components.Service;
    using Devices;
    using Devices.Components;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class DeviceUseCaseService : IDeviceUseCaseService
    {
        private readonly string _pathDeviceConfigs;
        private readonly string _pathConfig;

        private readonly List<Device> _devices = new List<Device>();

        private readonly IDriverUseCaseService _driverService;

        private DeviceManagerConfig _deviceManagerConfig;
        private readonly ILogger _logger;

        public DeviceUseCaseService(IDriverUseCaseService driverService)
        {
            _pathConfig = Path.GetFullPath(Path.Combine("Configs"));
            Directory.CreateDirectory(_pathConfig);
            _pathDeviceConfigs = Path.GetFullPath(Path.Combine($"Configs", "Devices"));
            Directory.CreateDirectory(_pathDeviceConfigs);

            try
            {
                _deviceManagerConfig = JsonConvert.DeserializeObject<DeviceManagerConfig>(File.ReadAllText(Path.Combine(_pathConfig, "Configuration.json")));
            }
            catch (Exception exception)
            {
                _deviceManagerConfig = new DeviceManagerConfig();
                File.WriteAllText(Path.Combine(_pathConfig, "Configuration.json"),
                    JsonConvert.SerializeObject(_deviceManagerConfig, Formatting.Indented));
            }
            
            _logger = new LoggerConfiguration().MinimumLevel.Debug()
                                               .WriteTo.Console()
                                               .WriteTo.File(
                                                   path: Path.Combine("Logs", nameof(DeviceUseCaseService), ".txt"),
                                                   rollingInterval: RollingInterval.Day,
                                                   fileSizeLimitBytes: 1024 * 1024,
                                                   rollOnFileSizeLimit: true,
                                                   retainedFileCountLimit: 7,
                                                   shared: true,
                                                   restrictedToMinimumLevel: LogEventLevel.Debug)
                                               .CreateLogger();

            _driverService = driverService;
        }

        public Device[] GetDevices() => _devices.ToArray();

        public async Task ReadDeviceConfigs(AppDbContext db)
        {
            var files = Directory.GetFiles(_pathDeviceConfigs, "*.json");

            if (files.Length > 0)
            {
                _logger.Information($"Device config {files.Length} found.");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    try
                    {
                        _logger.Information($"Device config {fileInfo.Name} load...");
                        var device = JsonConvert.DeserializeObject<DeviceConfig>(await File.ReadAllTextAsync(file));

                        device.Id = _devices.Select(x => x.Config.Id).Count() + 1;
                        _devices.Add(CreateDevice(device, db));

                        _logger.Information($"Device config {fileInfo.Name} loaded.");
                    }
                    catch (Exception exception)
                    {
                        _logger.Fatal(exception.Message);
                        _logger.Fatal(exception.StackTrace);
                        _logger.Error($"Device config {fileInfo.Name} load is fail! Skip...");
                    }
                }
            }
            else
            {
                _logger.Warning("Device config not found!");
            }
        }
        
        public async Task<Device> AddDevice(DeviceConfig device, AppDbContext db)
        {
            await File.AppendAllTextAsync(Path.Combine(_pathDeviceConfigs, $"{device.Name}.json"),
                JsonConvert.SerializeObject(device, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            _logger.Information($"Device configuration {device.Name} save.");

            var newDevice = CreateDevice(device, db);
            _devices.Add(newDevice);

            return newDevice;
        }

        public async Task RemoveDevice(int id)
        {
            var deviceConfig = _devices.Select(x => x.Config)
                                       .Single(x => x.Id == id);
            var device = _devices.Single(x => x.Config == deviceConfig);

            await device.StopAsync();
            _devices.Remove(device);

            File.Delete(Path.Combine(_pathDeviceConfigs, $"{deviceConfig.Name}.json"));
            _logger.Information($"Device configuration {deviceConfig.Name} deleted!");
        }

        public async Task UpdateDevice(int id, DeviceConfig afterDevice)
        {
            var beforeDevice = _devices.Select(x => x.Config)
                                       .Single(x => x.Id == id);
            var device = _devices.Single(x => x.Config == beforeDevice);

            try
            {
                device.Config = afterDevice;
                _logger.Information($"Device configuration Parser Equals...");
                if (afterDevice.Parser?.Equals(beforeDevice.Parser) is false)
                {
                    _logger.Information($"Device configuration Parser change!");
                    var afterParser = _driverService.GetParser(afterDevice.Parser.FullName);
                    afterParser.Logger = device.Logger;
                    device.Parser = afterParser;
                }
                _logger.Information($"Device configuration Connection Equals...");
                if (afterDevice.Connection?.Equals(beforeDevice.Connection) is false)
                {
                    _logger.Information($"Device configuration Connection change!");
                    await device.StopAsync();
                    await device.StartAsync();
                }
                _logger.Information($"Device configuration Logger Equals...");
                if (afterDevice.Logger?.Equals(beforeDevice.Logger) is false)
                {
                    _logger.Information($"Device configuration Logger change!");
                    // todo change logger;
                }

                await File.WriteAllTextAsync(Path.Combine(_pathDeviceConfigs, $"{afterDevice.Name}.json"),
                    JsonConvert.SerializeObject(afterDevice, Formatting.Indented, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                _logger.Debug(exception.StackTrace);
            }
        }

        public DeviceManagerConfig GetSettings() => _deviceManagerConfig;

        public async Task UpdateSettings(DeviceManagerConfig configuration)
        {
            _deviceManagerConfig = configuration;

            await File.WriteAllTextAsync(Path.Combine(_pathConfig, "Configuration.json"),
                JsonConvert.SerializeObject(_deviceManagerConfig, Formatting.Indented));
        }
        
        private Device CreateDevice(DeviceConfig config, AppDbContext db)
        {
            var deviceLogger = new DeviceLogs(50);
            var logger = new LoggerConfiguration().MinimumLevel.Debug()
                                                  .WriteTo.Console()
                                                  .WriteTo.File(
                                                      path: Path.Combine("Logs", config.Name, ".txt"),
                                                      rollingInterval: RollingInterval.Day,
                                                      fileSizeLimitBytes: 1024 * 1024,
                                                      rollOnFileSizeLimit: true,
                                                      retainedFileCountLimit: 7,
                                                      shared: true)
                                                  .WriteTo.Sink(deviceLogger)
                                                  .CreateLogger();

            var device = new Device
            {
                Config = config,
                Logger = logger,
                Connection = config.Connection?.ConnectionType switch
                {
                    ConnectionTypes.Network => new NetworkConnect(logger, config.Connection.Network),
                    ConnectionTypes.Serial => new SerialConnect(logger, config.Connection.Serial),
                    ConnectionTypes.FileSystem => new FileSystemConnect(logger, config.Connection.FileSystem),
                    _ => null
                },
                Parser = null,
                DeviceService = new DeviceService(logger, _deviceManagerConfig.Address, config.SystemName, config.DriverSystemName, db),
                DeviceLogs = deviceLogger
            };

            return device;
        }

        public async Task RetrySendTestResult(int id, int testResultId)
        {
            var device = _devices.Single(x => x.Config.Id == id);

            await device.RetrySendTestResult(testResultId);
        }
    }
}