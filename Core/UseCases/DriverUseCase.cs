namespace Core.UseCases
{
    using Configurations.Device.Driver;
    using DriverBase;
    using System;
    using System.Linq;
    using UseCaseServices;

    public sealed class DriverUseCase : IDriverUseCase
    {
        private readonly IDriverUseCaseService _driverUseCaseService;
        private readonly IDeviceUseCaseService _deviceUseCaseService;

        public DriverUseCase(
            IDriverUseCaseService driverUseCaseService,
            IDeviceUseCaseService deviceUseCaseService)
        {
            _driverUseCaseService = driverUseCaseService;
            _deviceUseCaseService = deviceUseCaseService;
        }

        public void Add(string[] fileNames)
        {
            _driverUseCaseService.Add(fileNames);
            Console.WriteLine($"Add");
            var parserFullNames = _driverUseCaseService.GetDriversInfo()
                                                       .Where(x => fileNames.Contains(x.FileName))
                                                       .SelectMany(x => x.Parsers.Select(y => y.FullName))
                                                       .ToArray();
            Console.WriteLine($"parserFullNames:{parserFullNames.Length}");
            foreach (var device in _deviceUseCaseService
                                   .GetDevices()
                                   .Where(x => parserFullNames.Contains(x.Configuration.Driver.Parser.FullName)))
            {
                Console.WriteLine($"set parser for device");
                var parser = _driverUseCaseService.GetParser(device.Configuration.Driver.Parser.FullName);
                parser.Logger = device.Logger;
                device.Parser = parser;
            }
        }

        public void Remove(string[] fileNames)
        {
            var parserFullNames = _driverUseCaseService.GetDriversInfo()
                                                       .Where(x => fileNames.Contains(x.FileName))
                                                       .SelectMany(x => x.Parsers.Select(y => y.FullName))
                                                       .ToArray();
            foreach (var device in _deviceUseCaseService
                                   .GetDevices()
                                   .Where(x => parserFullNames.Contains(x.Configuration.Driver.Parser.FullName)))
            {
                device.Parser = null;
            }

            _driverUseCaseService.Remove(fileNames);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public DriverInfo[] GetDriversInfo() => _driverUseCaseService.GetDriversInfo();

        public ParserInfo[] GetParsers() => _driverUseCaseService.GetParsers();

        public ParserInfo[] GetParsersByDriver(string fileName) => _driverUseCaseService.GetParsersByDriver(fileName);

        public IParser GetParser(string fullName) => _driverUseCaseService.GetParser(fullName);
    }
}