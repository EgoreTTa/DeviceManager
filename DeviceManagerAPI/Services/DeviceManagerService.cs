namespace DeviceManagerAPI.Services
{
    using DeviceManager;
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DeviceManagerService : BackgroundService
    {
        private readonly IDeviceManager _useService;

        public DeviceManagerService(IDeviceManager useService)
        {
            _useService = useService;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _useService.StartAsync(token);
        }
    }
}