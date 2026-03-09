namespace DeviceManagerAPI.Services
{
    using DeviceManager;
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DeviceManagerService : BackgroundService
    {
        private readonly IDeviceManager _service;

        public DeviceManagerService(IDeviceManager service) => _service = service;

        protected override Task ExecuteAsync(CancellationToken token) => _service.StartAsync(token);
    }
}