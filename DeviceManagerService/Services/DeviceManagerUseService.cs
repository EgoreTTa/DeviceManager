namespace DeviceManagerService.Services
{
    using Configurations.Device;
    using Configurations.Device.Connection;
    using Configurations.Device.Driver;
    using DataAccess;
    using DriverBase;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;
    using System.Threading;
    using System.Threading.Tasks;
    using Others;

    public class DeviceManagerUseService : IDeviceManagerUseService
    {
        private readonly Logger _logger;

        private readonly List<DeviceConfiguration> _devices = new List<DeviceConfiguration>()
        {
            new DeviceConfiguration()
            {
                Id = 1,
                Name = "DeviceName1",
                SystemName = "DeviceSystemName1",
                DriverConfiguration = new DriverConfiguration()
                {
                    Name = "DriverTest.DriverTest",
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
                        StopBits = StopBits.One
                    },
                    FileSystem = new FileSystemConnection()
                    {
                        FolderToRead = "",
                        FolderToWrite = ""
                    }
                }
            }
        };
        private readonly List<(string, IDriver)> _drivers = new List<(string, IDriver)>();
        private readonly List<HelpTracking> _trackings = new List<HelpTracking>();

        public DeviceManagerUseService()
        {
            _logger = new LoggerConfiguration().WriteTo
                                               .Console(LogEventLevel.Information)
                                               .WriteTo
                                               .File(
                                                   $"Logs{Path.DirectorySeparatorChar}" +
                                                   $"DeviceManager{Path.DirectorySeparatorChar}" +
                                                   $".txt",
                                                   rollingInterval: RollingInterval.Day,
                                                   fileSizeLimitBytes: 1024 * 1024,
                                                   rollOnFileSizeLimit: true,
                                                   retainedFileCountLimit: 7,
                                                   shared: true, 
                                                   restrictedToMinimumLevel: LogEventLevel.Information)
                                               .CreateLogger();
            ReadDeviceConfigurations();
            LoadDrivers(_devices.Select(x => x.DriverConfiguration).ToArray());
        }

        private void ReadDeviceConfigurations()
        {
            Directory.CreateDirectory("Devices");
            var files = Directory.GetFiles("Devices", "*.json");
            if (files.Length == 0)
            {
                _logger.Warning("Device configurations not found!");
                foreach (var deviceConfiguration in _devices)
                {
                    File.AppendAllText($"Devices{Path.DirectorySeparatorChar}{deviceConfiguration.Name}.json",
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
                        _logger.Information($"Device configuration load...");
                        var device = JsonConvert.DeserializeObject<DeviceConfiguration>(File.ReadAllText(file));
                        if (_devices.Any(x => x.Id == device.Id))
                        {
                            device.Id = _devices.Select(x => x.Id).Max() + 1;
                            File.WriteAllText(
                                $"Devices{Path.DirectorySeparatorChar}{file}",
                                JsonConvert.SerializeObject(device, Formatting.Indented));
                        }

                        _devices.Add(device);
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
            var files = driverConfigurations.Select(x => $"Drivers{Path.DirectorySeparatorChar}{x.Name}{(x.Name.Contains(".dll") is false ? ".dll" : string.Empty)}");
            foreach (var file in files)
            {
                if (File.Exists($"{file}") is false)
                {
                    _logger.Warning($"Driver {file} not found!");
                    continue;
                }

                _logger.Information($"Driver {file} loading...");
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var peReader = new PEReader(stream);

                var reader = peReader.GetMetadataReader();

                foreach (var handle in reader.TypeDefinitions)
                {
                    var definition = reader.GetTypeDefinition(handle);

                    var nameSpace = reader.GetString(definition.Namespace);
                    var name = reader.GetString(definition.Name);

                    if (name != "Driver") continue;

                    _logger.Information($"{file}=>{nameSpace}.{name}");

                    var driver = LoadingDrivers(file, $"{nameSpace}.{name}");
                    if (driver != null)
                    {
                        _drivers.Add(($"{nameSpace}.{name}", driver));
                        _logger.Information($"Driver {file} loaded.");
                    }
                }
            }
        }

        private IDriver LoadingDrivers(string fileName, string driverType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var assembly = assemblies.FirstOrDefault(x => x.ManifestModule.GetType(driverType) != null);
            if (assembly != null)
            {
                _logger.Warning($"Build {driverType} was downloaded previously");
                return null;
            }

            assembly = Assembly.LoadFrom(fileName);

            var type = assembly.GetType(driverType);
            if (type == null)
            {
                _logger.Warning($"{driverType} not found");
                return null;
            }

            var instance = Activator.CreateInstance(type);

            return instance as IDriver;
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

        public Task<DriverConfiguration[]> GetDrivers(CancellationToken token = default)
        {
            var drivers = _drivers.Select(x => new DriverConfiguration
            {
                Name = x.Item2.SupportedDevices.First(),
                Parsers = x.Item2.Parsers.Values.ToArray()
            });

            return Task.FromResult(drivers.ToArray());
        }

        public Task<DriverConfiguration> GetDrive(int id, CancellationToken token = default)
        {
            return null;
        }

        public Task AddDevice(DeviceConfiguration device, CancellationToken token = default)
        {
            File.AppendAllText($"Devices{Path.DirectorySeparatorChar}{device.Id}.json",
                JsonConvert.SerializeObject(device, Formatting.Indented));
            _logger.Information($"Device configuration {device.Name} save.");
            
            _devices.Add(device);

            return Task.CompletedTask;
        }

        public Task RemoveDevice(int id, CancellationToken token = default)
        {
            var device = _devices.Find(x => x.Id == id);
            File.Delete($"Devices{Path.DirectorySeparatorChar}{device.Id}.json");
            _logger.Information($"Device configuration {device.Name} deleted!");

            _devices.Remove(device);

            return Task.CompletedTask;
        }

        public Task UpdateDevice(int id, DeviceConfiguration device, CancellationToken token = default)
        {
            _devices[_devices.IndexOf(_devices.Find(x => x.Id == id))] = device;

            File.WriteAllText($"Devices{Path.DirectorySeparatorChar}{device.Id}",
                JsonConvert.SerializeObject(device, Formatting.Indented));

            return Task.CompletedTask;
        }

        private async Task StartDevice(CancellationToken token)
        {
            var deviceConfiguration = _devices.First();
            var device = new Device()
            {
                Logger = new LoggerConfiguration().WriteTo
                                                  .Console(LogEventLevel.Information)
                                                  .WriteTo
                                                  .File($"Logs{Path.DirectorySeparatorChar}" +
                                                        $"{deviceConfiguration.Name}{Path.DirectorySeparatorChar}" +
                                                        $".txt",
                                                      rollingInterval: RollingInterval.Day,
                                                      fileSizeLimitBytes: 1024 * 1024,
                                                      rollOnFileSizeLimit: true,
                                                      retainedFileCountLimit: 7,
                                                      shared: true,
                                                      restrictedToMinimumLevel: LogEventLevel.Information)
                                                  .CreateLogger(),
                Configuration = deviceConfiguration
            };
            await device.StartAsync(token);
        }
    }

    public class Device
    {
        public ILogger Logger { get; set; }
        public DeviceConfiguration Configuration { get; set; }

        public async Task StartAsync(CancellationToken token)
        {
            var dataAccess = new DataAccess("http://192.168.241.141/med2des/ws/lis", "SystemName_Device0001", "SystemName_Driver0001");

            var driver = Activator.CreateInstance(
                AppDomain.CurrentDomain
                         .GetAssemblies()
                         .Single(x => x.GetType(Configuration.DriverConfiguration.Name) != null)
                         .GetType()) as IDriver;
           
            var parser = driver.Parsers.First().Key;

            var serial = new SerialPort($"{Configuration.ConnectionConfiguration.Serial.PortName}");
            serial.Open();
            Logger.Warning($"serial {Configuration.ConnectionConfiguration.Serial.PortName} open!");

            var buffer = new byte[2048];
            while (token.IsCancellationRequested is false)
            {
                try
                {
                    var count = await serial.BaseStream.ReadAsync(buffer, token);
                    var bytes = buffer.Take(count).ToArray();
                    Logger.Warning($"serial {Configuration.ConnectionConfiguration.Serial.PortName} receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                    if (parser.TryParse(bytes, out var samples, out var send)) //todo parse ok?
                    {
                        if (send != null)
                        {
                            await serial.BaseStream.WriteAsync(send, token);
                        }
                        if (samples.Any(x => x.Results != null))
                        {
                            await dataAccess.SetDeviceResults(samples);
                        }
                        else
                        {
                            var directiveLines = await dataAccess.GetDirectiveLinesByBarcodes(
                                samples.Select(x => x.SampleCode)
                                       .ToArray());

                            if (parser.TryParseOrder(directiveLines, out var order))
                            {
                                await serial.BaseStream.WriteAsync(order, token);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Fatal(exception.Message);
                    Logger.Error(exception.StackTrace);
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            }

            Logger.Warning($"serial {Configuration.ConnectionConfiguration.Serial.PortName} close!");
        }

        public async Task StopAsync(CancellationToken token) { }
    }
}