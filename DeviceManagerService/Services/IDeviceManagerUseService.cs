namespace DeviceManagerService.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Others;

    public interface IDeviceManagerUseService
    {
        public Task StartAsync(CancellationToken token);
        public Task<Device[]> GetDevices(CancellationToken token = default);
        public Task AddDevice(Device device, CancellationToken token = default);
        public Task RemoveDevice(int id, CancellationToken token = default);
    }
}