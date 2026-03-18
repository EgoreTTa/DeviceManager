namespace Core.UseCases
{
    using Configurations;
    using Configurations.Device;
    using Devices;
    using DriverBase.DTOs;
    using Entities;
    using System.Threading.Tasks;

    public interface IDeviceUseCase
    {
        Device[] GetDevices();
        DeviceManagerConfig GetSettings();
        Task ReadDeviceConfigs();
        Task<DeviceManagerEvent[]> GetEvents();
        Task<DeviceManagerEvent> AddDevice(DeviceConfig device);
        Task<DeviceManagerEvent> RemoveDevice(int id);
        Task<DeviceManagerEvent> UpdateDevice(int id, DeviceConfig device);
        Task<DeviceManagerEvent> UpdateSettings(DeviceManagerConfig configuration);
        Task<DeviceManagerEvent> RetrySendTestResult(int id, int testResultId);
        Task<TestResultDTO[]> GetTestResults(int id);
    }
}