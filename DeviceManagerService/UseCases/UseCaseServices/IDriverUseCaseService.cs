namespace DeviceManager.UseCases.UseCaseServices
{
    using Configurations.Device.Driver;
    using DriverBase;

    public interface IDriverUseCaseService
    {
        DriverInfo[] GetDriversInfo();
        void Add(string[] fileNames);
        void Remove(string[] fileNames);
        ParserInfo[] GetParsers();
        ParserInfo[] GetParsersByDriver(string fileName);
        IParser GetParser(string fullname);
    }
}