namespace DeviceManagerService
{
    using Microsoft.Extensions.Hosting;
    using Services;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DeviceManagerService : BackgroundService
    {
        private readonly IDeviceManagerUseService _useService;

        public DeviceManagerService(IDeviceManagerUseService useService)
        {
            _useService = useService;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            Console.WriteLine("DeviceManagerService ExecuteAsync");
            await _useService.StartAsync(token);
        }
    }
}