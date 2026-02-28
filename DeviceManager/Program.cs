namespace DeviceManager
{
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Reflection.PortableExecutable;
    using System.Text.Json;

    public class Program
    {
        private static readonly List<IDriver> Drivers = new List<IDriver>();

        public static void Main()
        {
            var driversPath = $"Drivers{Path.DirectorySeparatorChar}";
            var deviceRegistries = LoadingDeviceFiles();

            foreach (var file in Directory.GetFiles(driversPath))
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var peReader = new PEReader(stream);

                var reader = peReader.GetMetadataReader();

                foreach (var handle in reader.TypeDefinitions)
                {
                    var definition = reader.GetTypeDefinition(handle);

                    var nameSpace = reader.GetString(definition.Namespace);
                    var name = reader.GetString(definition.Name);

                    if (name != "Driver") continue;

                    Console.WriteLine($"{DateTime.Now}\t{file}=>{nameSpace}.{name}");

                    var driver = LoadingDrivers(file, $"{nameSpace}.{name}");
                    Drivers.Add(driver);
                    if (driver != null)
                    {
                        try
                        {
                            Console.WriteLine($"{DateTime.Now}\t{file}=>{nameSpace}.{name}.Start()");
                            // driver.SetConnectionType(2);
                            // driver.SetConnectionSettings("asdf", "asdf");
                            driver.SetRegistry(deviceRegistries.First());
                            driver.Start();
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine($"{DateTime.Now}\t{exception.Message}");
                            Console.WriteLine($"{DateTime.Now}\t{exception.StackTrace}");
                        }
                    }
                }
            }
        }

        private static DeviceRegistry[] LoadingDeviceFiles()
        {
            Directory.CreateDirectory("Devices");
            var deviceFiles = Directory.GetFiles("Devices", "*.json");
            Console.WriteLine($"{DateTime.Now}\t" + $"{deviceFiles.Length} registration files were uploaded");
            if (deviceFiles.Length == 0)
            {
                var sampleDeviceRegistry = new DeviceRegistry()
                {
                    DeviceName = nameof(DeviceRegistry.DeviceName),
                    DeviceSystemName = nameof(DeviceRegistry.DeviceSystemName),
                    DriverName = nameof(DeviceRegistry.DriverName),
                    DriverSystemName = nameof(DeviceRegistry.DriverSystemName),
                    Encoding = nameof(DeviceRegistry.Encoding),
                    ConnectionType = ConnectionTypes.TcpIp,
                    TcpIp = new TcpIpConnection()
                    {
                        Mode = TcpIpModes.Server,
                        IpEndPoint = new IPEndPoint(IPAddress.Loopback, 5000)
                    },
                    Serial = new SerialConnection()
                    {
                        PortName = "COM1",
                        BaudRate = 9600,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Parity = Parity.None,
                    },
                    FileSystem = new FileSystemConnection()
                    {
                        FolderToWrite = $"Devices{Path.DirectorySeparatorChar}" +
                                        $"{nameof(DeviceRegistry.DeviceName)}{Path.DirectorySeparatorChar}" +
                                        $"ToWrite{Path.DirectorySeparatorChar}",
                        FolderToRead = $"Devices{Path.DirectorySeparatorChar}" +
                                       $"{nameof(DeviceRegistry.DeviceName)}{Path.DirectorySeparatorChar}" +
                                       $"ToRead{Path.DirectorySeparatorChar}",
                    },
                };

                var content = JsonSerializer.Serialize(sampleDeviceRegistry);
                File.WriteAllText($"Devices{Path.DirectorySeparatorChar}{nameof(sampleDeviceRegistry)}", content);
                Console.WriteLine($"{DateTime.Now}\t" + "Created sample DeviceRegistry.json.");
            }
            var deviceRegistries = new List<DeviceRegistry>();
            foreach (var deviceFile in deviceFiles)
            {
                Console.WriteLine($"{DateTime.Now}\t" + $"{deviceFile.Split('\\').Last()} registration files were uploaded");
                var deviceRegistryFile = File.ReadAllText(deviceFile);
                try
                {
                    var deviceRegistry = JsonSerializer.Deserialize<DeviceRegistry>(deviceRegistryFile);
                    deviceRegistries.Add(deviceRegistry);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now}\t" + exception.Message);
                }
            }

            return deviceRegistries.ToArray();
        }

        private static IDriver LoadingDrivers(string fileName, string driverType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var assembly = assemblies.FirstOrDefault(x => x.ManifestModule.GetType(driverType) != null);
            if (assembly != null)
            {
                Console.WriteLine($"{DateTime.Now}\t" + $"Build {driverType} was downloaded previously");
                return null;
            }
            else
            {
                assembly = Assembly.LoadFrom(fileName);

                var type = assembly.GetType(driverType);
                if (type == null)
                {
                    Console.WriteLine($"{DateTime.Now}\t" + $"{driverType} not found");
                    return null;
                }

                var instance = Activator.CreateInstance(type);

                return instance as IDriver;
            }
        }
    }
}
