namespace DeviceManagerService.Services
{
    using Configurations.Device;
    using Configurations.Device.Connection;
    using Configurations.Device.Driver;
    using Newtonsoft.Json;
    using Others;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection.PortableExecutable;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Reflection.Metadata;

    public class DeviceManagerUseService : IDeviceManagerUseService
    {
        private readonly Logger _logger;

        private readonly List<DeviceConfiguration> _devices = new List<DeviceConfiguration>()
        {
            new DeviceConfiguration()
            {
                Id = 1,
                DeviceName = "DeviceName1",
                DeviceSystemName = "DeviceSystemName1",
                DriverConfiguration = new DriverConfiguration()
                {
                    DriverName = "MedonicM20.dll",
                    DriverSystemName = "DriverSystemName1",
                    Encoding = "utf-8"
                },
                ConnectionConfiguration = new ConnectionConfiguration()
                {
                    ConnectionType = ConnectionTypes.Network,
                    Network = new NetworkConnection()
                    {
                        Mode = NetworkModes.Server,
                        Address = "127.0.0.1",
                        Port = "5050"
                    }
                }
            }
        };
        private readonly List<DriverConfiguration> _drivers = new List<DriverConfiguration>();

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
                    File.AppendAllText($"Devices{Path.DirectorySeparatorChar}{deviceConfiguration.DeviceName}.json",
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
                        _devices.Add(JsonConvert.DeserializeObject<DeviceConfiguration>(File.ReadAllText(file)));
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
            var files = driverConfigurations.Select(x => $"Drivers{Path.DirectorySeparatorChar}{x.DriverName}{(x.DriverName.Contains(".dll") is false ? ".dll" : string.Empty)}");
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
                _logger.Information($"MetadataVersion:{reader.MetadataVersion}");

                foreach (var handle in reader.TypeDefinitions)
                {
                    var definition = reader.GetTypeDefinition(handle);

                    var nameSpace = reader.GetString(definition.Namespace);
                    var name = reader.GetString(definition.Name);

                    _logger.Information($"definition.Namespace:{nameSpace}");
                    _logger.Information($"definition.Name:{name}");
                }
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            _logger.Information("DeviceManagerService: start");

            try
            {
                while (token.IsCancellationRequested is false)
                {
                    _logger.Information($"Tracking start...");

                    await Task.Run(() => { Thread.Sleep(TimeSpan.FromMinutes(1)); },
                        token);
                    _logger.Warning($"Device stop!");

                    _logger.Information($"Tracking end.");
                }
            }
            catch (Exception exception)
            {
                _logger.Fatal(exception.Message);
                _logger.Error(exception.StackTrace);
            }
        }

        public Task<Device[]> GetDevices(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task AddDevice(Device device, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveDevice(int id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}