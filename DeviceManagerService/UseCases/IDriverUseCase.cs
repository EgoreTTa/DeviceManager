namespace DeviceManager.UseCases
{
    using Configurations.Device.Driver;
    using DriverBase;

    public interface IDriverUseCase
    {
        DriverInfo[] GetDriversInfo();
        void Add(string[] fileNames);
        void Remove(string[] fileNames);
        ParserInfo[] GetParsers();
        ParserInfo[] GetParsersByDriver(string fileName);
        IParser GetParser(string fullname);
    }
}