namespace Core.Devices.Components.Service
{
    using DriverBase.DTOs;
    using Infrastructure;
    using Infrastructure.DTOs.LIS;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class DeviceService
    {
        private readonly string _deviceSystemName;
        private readonly string _driverSystemName;
        private readonly AppDbContext _context;

        private DataAccess _dataAccess;
        private DeviceInfoDto _deviceInfoDto;
        private TestCollationDto[] _testCollationDto;
        private MeasureUnitDto[] _measureUnitDtos;
        private EnumValueDto[] _enumValueDtos;
        private AntibioticDto[] _antibioticDtos;
        private BacteriumDto[] _bacteriumDtos;
        private BiomaterialDto[] _biomaterialDtos;

        public ILogger Logger { get; set; }
        public TestCollationDto[] TestCollationDto => _testCollationDto.ToArray();

        public DeviceService(ILogger logger, string url, string deviceSystemName, string driverSystemName, AppDbContext context)
        {
            Logger = logger;
            _context = context;
            _deviceSystemName = deviceSystemName;
            _driverSystemName = driverSystemName;
            _dataAccess = new DataAccess(logger, url);
        }

        public async Task GetComparisons()
        {
            _deviceInfoDto = await _dataAccess.GetDeviceInfo(_driverSystemName);
            _testCollationDto = await _dataAccess.GetTestCollations(_driverSystemName);
            _measureUnitDtos = await _dataAccess.GetMeasureUnits(_driverSystemName);
            _enumValueDtos = await _dataAccess.GetEnumValues(_driverSystemName);
        }

        public async Task SetDeviceResults(TestResultDTO[] results)
        {
            foreach (var dto in results)
            {
                dto.DeviceSystemName = _deviceSystemName;
                dto.DriverSystemName = _driverSystemName;
                dto.Status = "waiting";
                
                if (dto.Id == 0) _context.TestResults.Add(dto);
            }
            await _context.SaveChangesAsync();


            var resultsToErrors = new List<TestResultDTO>();

            var testsForSaveToSend = results
                                     .Where(x =>
                                         _testCollationDto.Select(y => y.Code)
                                                          .Contains(x.TestCode))
                                     .ToArray();

            var testForSaveToErrors = results
                                      .Except(testsForSaveToSend)
                                      .ToArray();

            foreach (var testResults in results)
            {
                if (testForSaveToErrors.Length > 0)
                    resultsToErrors.AddRange(testForSaveToErrors);

                if (testsForSaveToSend.Length > 0)
                {
                    var deviceOrderDtos = await GetDirectiveLinesByBarcodes(new[] { testResults.SampleCode });

                    foreach (var result in testsForSaveToSend)
                    {
                        Logger.Information($"result.TestCode:{result.TestCode}\t" +
                                           $"result.MuCode:{result.MuCode}");

                        var testCollation = _testCollationDto.SingleOrDefault(x => x.Code == result.TestCode)
                                                             ?.SystemEntityId;
                        var measureUnit = _measureUnitDtos.SingleOrDefault(x => x._code == result.MuCode)
                                                          ?._systemEntityId;

                        if (string.IsNullOrEmpty(testCollation) is false
                            &&
                            string.IsNullOrEmpty(measureUnit) is false)
                        {
                            foreach (var deviceOrderDto in deviceOrderDtos)
                            {
                                var directiveLines = new List<DirectiveLine>();

                                // var testsAfterSave = results.SelectMany(x => deviceOrderDto._directionLines)
                                //                             .ToArray();

                                foreach (var directionLine in deviceOrderDto._directionLines)
                                {
                                    if (directionLine._requestedbarcode == testResults.SampleCode)
                                    {
                                        foreach (var testDTO in directionLine._testDTOs)
                                        {
                                            Logger.Information($"deviceOrderDto.id:{deviceOrderDto._id}\t" +
                                                               $"|directionLine.id:{directionLine._id}\t" +
                                                               $"|test.id:{testDTO._testId}\t" +
                                                               $"|test.muId:{testDTO._muId}\t" +
                                                               $"|testCollation:{testCollation}\t" +
                                                               $"|measureUnit:{measureUnit}");
                                            if (testDTO._testId == testCollation
                                                &&
                                                testDTO._muId == measureUnit)
                                            {
                                                testDTO._value = result.Value;
                                                Logger.Information($"Save testCollation:{testCollation}\t" +
                                                              $"|value {testDTO._value}\t" +
                                                              $"|measureUnit:{measureUnit}");

                                                directiveLines.Add(new DirectiveLine()
                                                {
                                                    Id = directionLine._id,
                                                    CreatorSharedId = directionLine._creatorsharedid,
                                                    ResearchResults = new[]
                                                    {
                                                    new ResearchResult()
                                                    {
                                                        ResultTypeData = testDTO._resultTypeData,
                                                        TestId = testDTO._testId,
                                                        Value = testDTO._value,
                                                        MUId = testDTO._muId
                                                    }
                                                }
                                                });
                                            }
                                        }
                                    }
                                }

                                if (directiveLines.Count > 0)
                                {
                                    var saveDeviceResults = new SaveDeviceResultsRequest()
                                    {
                                        DirectiveLines = directiveLines.ToArray(),
                                        DeviceSystemName = _deviceInfoDto.SystemName
                                    };

                                    var status = await _dataAccess.SaveDeviceResults(saveDeviceResults);
                                    //
                                    // foreach (var directionLine in status)
                                    // {
                                    //     foreach (var directionLineTest in directionLine._tests)
                                    //     {
                                    //         testsForSaveToSend.Remove(directionLineTest);
                                    //     }
                                    // }
                                }
                            }
                        }
                    }
                }
            }

            if (resultsToErrors.Count > 0)
            {
                Logger.Warning("Save To Errors");
            }
        }

        public async Task<DeviceOrderDTO[]> GetDirectiveLinesByBarcodes(string[] barcodes)
        {
            var deviceOrderDtos = await _dataAccess.GetDirectiveLinesByBarcodes(
                _deviceInfoDto.SystemName,
                barcodes,
                false,
                _deviceInfoDto.LpuId);
            Logger.Information($"Получено {deviceOrderDtos.Length} Device Orders для Barcodes=\"{string.Join(", ", barcodes)}\"");

            foreach (var orderDto in deviceOrderDtos)
            {
                var tests = orderDto._directionLines.SelectMany(y =>
                                        y._testDTOs.Select(z => z._testId))
                                    .Distinct()
                                    .Intersect(_testCollationDto.Select(x => x.SystemEntityId))
                                    .ToArray();

                Logger.Information($"Сопоставлено {tests.Length} показателя для Barcode=\"{orderDto._directionLines.First()._samplebarcode}\"");
                foreach (var test in tests)
                {
                    Logger.Information($"Сопоставлено ID=\"{test}\" для Barcode=\"{orderDto._directionLines.First()._samplebarcode}\"");
                }
            }
            return deviceOrderDtos;
        }

        public async Task RetrySendTestResult(int id)
        {
            await _context.TestResults.LoadAsync();
            var testResult = _context.TestResults.Single(x => x.Id == id);
            await SetDeviceResults(new[] { testResult });
        }
    }
}