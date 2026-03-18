namespace API.Services
{
    using Core.UseCases;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DeviceManagerService : BackgroundService
    {
        private readonly IDeviceUseCase _device;
        private readonly IDriverUseCase _driver;

        public DeviceManagerService(
            IDeviceUseCase device,
            IDriverUseCase driver)
        {
            _device = device;
            _driver = driver;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _driver.Add(Directory.GetFiles(Path.Combine("Drivers"), "*.dll").Select(Path.GetFileName).ToArray());
            await _device.ReadDeviceConfigs();

            foreach (var device in _device.GetDevices())
            {
                try
                {
                    var parser = _driver.GetParser(device.Configuration.Driver.Parser.FullName);
                    parser.Logger = device.Logger;
                    if (device.Configuration.Driver.Parser.Options?.Length > 0)
                        parser.SetOptions(device.Configuration.Driver.Parser.Options);
                    device.Parser = parser;
                    await device.StartAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}