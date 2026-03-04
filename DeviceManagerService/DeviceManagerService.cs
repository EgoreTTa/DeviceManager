namespace DeviceManagerService
{
    using Microsoft.Extensions.Hosting;
    using Services;
    using System.Threading;
    using System.Threading.Tasks;

    public class DeviceManagerService : BackgroundService
    {
        private readonly IDeviceManagerUseService _useService;

        public DeviceManagerService(IDeviceManagerUseService useService)
        {
            _useService = useService;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _useService.StartAsync(token);
        }
    }
}