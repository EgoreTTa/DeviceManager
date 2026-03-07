namespace DeviceManagerService.Services
{
    using Configurations;
    using Configurations.Device;
    using Configurations.Device.Connection;
    using Configurations.Device.Driver;
    using DriverBase;
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

    public class DeviceManagerUseService : IDeviceManagerUseService
    {
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

        private DeviceManagerConfiguration _deviceManagerConfiguration;
        private ILogger _logger;

        public DeviceManagerUseService()
        {
            Directory.CreateDirectory($"Configs{Path.DirectorySeparatorChar}");

            _deviceManagerConfiguration = JsonConvert.DeserializeObject<DeviceManagerConfiguration>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Configuration.json"));
            _logger = new LoggerConfiguration().MinimumLevel.Debug()
                                               .WriteTo.Console()
                                               .WriteTo.File(
                                                   path: $"Logs{Path.DirectorySeparatorChar}" +
                                                         $"DeviceManager{Path.DirectorySeparatorChar}" +
                                                         $".txt",
                                                   rollingInterval: RollingInterval.Day,
                                                   fileSizeLimitBytes: 1024 * 1024,
                                                   rollOnFileSizeLimit: true,
                                                   retainedFileCountLimit: 7,
                                                   shared: true,
                                                   restrictedToMinimumLevel: LogEventLevel.Debug)
                                               .CreateLogger();

            ReadDeviceConfigurations();
            LoadDrivers(_devices.Select(x => x.DriverConfiguration).ToArray());
        }

        private void ReadDeviceConfigurations()
        {
            Directory.CreateDirectory($"Configs{Path.DirectorySeparatorChar}Devices");
            var files = Directory.GetFiles($"Configs{Path.DirectorySeparatorChar}Devices", "*.json");
            if (files.Length == 0)
            {
                _logger.Warning("Device configurations not found!");
                foreach (var deviceConfiguration in _devices)
                {
                    File.AppendAllText($"Configs{Path.DirectorySeparatorChar}Devices{Path.DirectorySeparatorChar}{deviceConfiguration.Name}.json",
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
                            File.WriteAllText(
                                $"Configs{Path.DirectorySeparatorChar}" +
                                $"Devices{Path.DirectorySeparatorChar}" +
                                $"{file}",
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
                        .Select(x => $"Drivers{Path.DirectorySeparatorChar}{x.Driver.FileName}{(x.Driver.FileName.Contains(".dll") is false ? ".dll" : string.Empty)}")
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
            _logger.Information("DeviceManagerService: start");


            while (token.IsCancellationRequested is false)
            {
                try
                {
                    _logger.Information($"Tracking start...");

                    await StartDevice(token);

                    _logger.Warning($"Device stop!");

                    _logger.Information($"Tracking end.");
                }
                catch (Exception exception)
                {
                    _logger.Fatal(exception.Message);
                    _logger.Error(exception.StackTrace);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            }

            _logger.Warning("DeviceManagerService: stop");
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

        public Task<DriverConfiguration> GetDrive(int id, CancellationToken token = default)
        {
            return null;
        }

        public Task AddDevice(DeviceConfiguration device, CancellationToken token = default)
        {
            File.AppendAllText($"Configs{Path.DirectorySeparatorChar}Devices{Path.DirectorySeparatorChar}{device.Id}.json",
                JsonConvert.SerializeObject(device, Formatting.Indented));
            _logger.Information($"Device configuration {device.Name} save.");
            
            _devices.Add(device);

            return Task.CompletedTask;
        }

        public Task RemoveDevice(int id, CancellationToken token = default)
        {
            var device = _devices.Find(x => x.Id == id);
            File.Delete($"Configs{Path.DirectorySeparatorChar}Devices{Path.DirectorySeparatorChar}{device.Id}.json");
            _logger.Information($"Device configuration {device.Name} deleted!");

            _devices.Remove(device);

            return Task.CompletedTask;
        }

        public Task UpdateDevice(int id, DeviceConfiguration device, CancellationToken token = default)
        {
            _devices[_devices.IndexOf(_devices.Find(x => x.Id == id))] = device;

            File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Devices{Path.DirectorySeparatorChar}{device.Id}",
                JsonConvert.SerializeObject(device, Formatting.Indented));

            return Task.CompletedTask;
        }

        private async Task StartDevice(CancellationToken token)
        {
            var deviceConfiguration = _devices.First();
            var device = new Device()
            {
                Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                                  .WriteTo.Console()
                                                  .WriteTo.File(
                                                      path: $"Logs{Path.DirectorySeparatorChar}" +
                                                            $"{deviceConfiguration.Name}{Path.DirectorySeparatorChar}" +
                                                            $".txt",
                                                      rollingInterval: RollingInterval.Day,
                                                      fileSizeLimitBytes: 1024 * 1024,
                                                      rollOnFileSizeLimit: true,
                                                      retainedFileCountLimit: 7,
                                                      shared: true)
                                                  .CreateLogger(),
                Configuration = deviceConfiguration
            };

            var parser = Activator.CreateInstance(_parsers.First().type) as IParser;
            if (parser == null) return;
            parser.Logger = device.Logger;

            await device.StartAsync(_deviceManagerConfiguration.LisUrl, parser, token);
        }
    }
}