namespace DeviceManager.UseCases.UseCaseServices
{
    using Configurations.Device.Driver;
    using DriverBase;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Loader;

    public sealed class DriverUseCaseService : IDriverUseCaseService
    {
        private readonly string _pathDrivers;
        private readonly List<DriverInfo> _driversInfo = new List<DriverInfo>();
        private readonly List<(AssemblyLoadContext context, Type type)> _contextAndType = new List<(AssemblyLoadContext, Type)>();
        private readonly ILogger _logger;

        public DriverUseCaseService()
        {
            _pathDrivers = Path.GetFullPath(Path.Combine("Drivers"));
            Directory.CreateDirectory(_pathDrivers);

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
        }

        public void Add(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var pathFile = Path.Combine(Directory.GetCurrentDirectory(), "Drivers", fileName);
                if (File.Exists($"{pathFile}") is false)
                {
                    _logger.Warning($"Driver {fileName} not found!");
                    continue;
                }

                if (_contextAndType.Select(x => x.context.Name).Any(x => x == fileName))
                {
                    _logger.Warning($"Conflict detected! Context {fileName} already loaded, to replace please unload first.");
                    continue;
                }

                _logger.Information($"Driver {fileName} loading...");

                try
                {
                    var context = new AssemblyLoadContext(fileName, true);

                    var bytes = File.ReadAllBytes(pathFile);
                    using var stream = new MemoryStream(bytes);
                    var assembly = context.LoadFromStream(stream);

                    var driverTypes = assembly.GetTypes()
                                              .Where(type =>
                                                  type.IsClass
                                                  &&
                                                  typeof(IParser).IsAssignableFrom(type));


                    _contextAndType.AddRange(driverTypes.Select(x => (context, x)));

                    var parsersFullName = driverTypes.Select(x => x.FullName).ToArray();

                    _driversInfo.Add(new DriverInfo()
                    {
                        FileName = fileName,
                        Parsers = parsersFullName.Select(fullName => new ParserInfo()
                                                 {
                                                     FullName = fullName,
                                                     Options = GetParser(fullName).GetOptions()
                                                 })
                                                 .ToArray(),
                    });

                    _logger.Information($"Driver {fileName} with parsers ({string.Join(", ", parsersFullName)}) loaded.");
                }
                catch (Exception exception)
                {
                    _logger.Error(exception.Message);
                    _logger.Debug(exception.StackTrace);
                    _logger.Error($"Driver {fileName} load is fail! Skip...");
                }
            }
        }

        public void Remove(string[] fileNames)
        {
            _logger.Information($"Drivers {string.Join(", ", fileNames)} upload...");

            var contextAndTypes = _contextAndType.Where(x => fileNames.Contains(x.context.Name))
                                                 .Distinct()
                                                 .ToArray();
            _logger.Debug($"contexts {string.Join(", ", contextAndTypes.Select(x => x.context.Name))}.");
            var contexts = contextAndTypes.Select(x => x.context).Distinct();
            foreach (var context in contexts) context.Unload();

            foreach (var context in contextAndTypes)
                _contextAndType.Remove(_contextAndType.Find(x => x.context == context.context));

            foreach (var driverInfo in _driversInfo.Where(x => fileNames.Contains(x.FileName)).ToArray())
                _driversInfo.Remove(driverInfo);

            _logger.Information($"Drivers {string.Join(", ", fileNames)} uploaded.");
        }

        public DriverInfo[] GetDriversInfo() => _driversInfo.ToArray();

        public ParserInfo[] GetParsers() => _driversInfo.SelectMany(x => x.Parsers).ToArray();

        public ParserInfo[] GetParsersByDriver(string fileName) => _driversInfo.Where(x => x.FileName == fileName)
                                                                               .SelectMany(x => x.Parsers)
                                                                               .ToArray();

        public IParser GetParser(string fullName)
        {
            _logger.Information($"Get parser {fullName}...");
            var type = _contextAndType.Select(x => x.type).Single(x => x.FullName == fullName);
            _logger.Information($"Get parser {fullName}.");
            return Activator.CreateInstance(type) as IParser;
        }
    }
}