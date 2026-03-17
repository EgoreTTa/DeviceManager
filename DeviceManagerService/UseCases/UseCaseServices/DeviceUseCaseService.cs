namespace DeviceManager.UseCases.UseCaseServices
{
    using Configurations;
    using Configurations.Device;
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

                        device.Id = _devices.Select(x => x.Configuration.Id).Count() + 1;
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
            var deviceConfig = _devices.Select(x => x.Configuration)
                                       .Single(x => x.Id == id);
            var device = _devices.Single(x => x.Configuration == deviceConfig);

            await device.StopAsync();
            _devices.Remove(device);

            File.Delete(Path.Combine(_pathDeviceConfigs, $"{deviceConfig.Id}.json"));
            _logger.Information($"Device configuration {deviceConfig.Name} deleted!");
        }

        public async Task UpdateDevice(int id, DeviceConfig afterDevice)
        {
            var beforeDevice = _devices.Select(x => x.Configuration)
                                       .Single(x => x.Id == id);
            var device = _devices.Single(x => x.Configuration == beforeDevice);

            if (afterDevice.Driver.Parser != beforeDevice.Driver.Parser)
            {
                var afterParser = _driverService.GetParser(afterDevice.Driver.Parser.FullName);
                afterParser.Logger = device.Logger;
                device.Parser = afterParser;
            }

            afterDevice.Id = id;
            device.Configuration = afterDevice;
            await File.WriteAllTextAsync(Path.Combine(_pathDeviceConfigs, $"{afterDevice.Name}.json"),
                JsonConvert.SerializeObject(afterDevice, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
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

            return new Device(logger, config, null, _deviceManagerConfig.Address, db, deviceLogger);
        }

        public async Task RetrySendTestResult(int id, int testResultId)
        {
            var device = _devices.Single(x => x.Configuration.Id == id);

            await device.RetrySendTestResult(testResultId);
        }
    }
}