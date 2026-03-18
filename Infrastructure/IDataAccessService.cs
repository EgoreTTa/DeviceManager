namespace Infrastructure
{
    using DTOs.LIS;
    using System.Threading.Tasks;

    public interface IDataAccessService
    {
        public Task<DeviceInfoDto> GetDeviceInfo(string driverSystemName);
        public Task<EnumValueDto[]> GetEnumValues(string driverSystemName);
        public Task<MeasureUnitDto[]> GetMeasureUnits(string driverSystemName);
        public Task<TestCollationDto[]> GetTestCollations(string driverSystemName);
        public Task<AntibioticDto[]> GetAntibiotics(string driverSystemName);
        public Task<BacteriumDto[]> GetBacteries(string driverSystemName);
        public Task<BiomaterialDto[]> GetBiomaterials(string driverSystemName);
        public Task<DeviceOrderDTO[]> GetDirectiveLinesByBarcodes(string deviceSystemName, string[] barcodes, bool autoSuggestBarcode, string lpu);
        public Task<StatusDto[]> SaveDeviceResults(SaveDeviceResultsRequest saveDeviceResultsRequest);
    }
}