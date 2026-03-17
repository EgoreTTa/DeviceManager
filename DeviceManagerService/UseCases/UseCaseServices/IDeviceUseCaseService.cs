namespace DeviceManager.UseCases.UseCaseServices
{
    using Configurations;
    using Configurations.Device;
    using Devices;
    using System.Threading.Tasks;

    public interface IDeviceUseCaseService
    {
        Device[] GetDevices();
        DeviceManagerConfig GetSettings();
        Task ReadDeviceConfigs(AppDbContext db);
        Task<Device> AddDevice(DeviceConfig device, AppDbContext db);
        Task RemoveDevice(int id);
        Task UpdateDevice(int id, DeviceConfig device);
        Task UpdateSettings(DeviceManagerConfig configuration);
        Task RetrySendTestResult(int id, int testResultId);
    }
}