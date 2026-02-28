namespace DataAccess
{
    using DTOs;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class DataAccess
    {
        private readonly string _deviceSystemName;
        private readonly string _driverSystemName;

        private IDataAccessService _accessService;

        private DeviceInfoDto _deviceInfoDto;
        private TestCollationDto[] _testCollationDto;
        private MeasureUnitDto[] _measureUnitDtos;
        private EnumValueDto[] _enumValueDtos;
        private AntibioticDto[] _antibioticDtos;
        private BacteriumDto[] _bacteriumDtos;
        private BiomaterialDto[] _biomaterialDtos;

        public DataAccess(string url, string deviceSystemName, string driverSystemName)
        {
            _deviceSystemName = deviceSystemName;
            _driverSystemName = driverSystemName;

            _accessService = new DataAccessService(url);

            try
            {
                _deviceInfoDto = _accessService.GetDeviceInfo(_driverSystemName).Result;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now}\t" + exception.Message);
                Console.WriteLine($"{DateTime.Now}\t" + exception.StackTrace);
            }

            if (_deviceInfoDto == null) return;

            _testCollationDto = _accessService.GetTestCollations(_driverSystemName).Result;
            _measureUnitDtos = _accessService.GetMeasureUnits(_driverSystemName).Result;
            _enumValueDtos = _accessService.GetEnumValues(_driverSystemName).Result;
            _antibioticDtos = _accessService.GetAntibiotics(_driverSystemName).Result;
            _bacteriumDtos = _accessService.GetBacteries(_driverSystemName).Result;
            _biomaterialDtos = _accessService.GetBiomaterials(_driverSystemName).Result;
        }

        public async Task<(string code, string id)[]> GetDriverTestCollations()
        {
            var result = new List<(string code, string id)>();
            foreach (var collationDto in _testCollationDto)
            {
                result.Add((collationDto._code, collationDto._systemEntityId));
            }

            return result.ToArray();
        }

        public async Task<DeviceOrderDTO[]> GetDirectiveLinesByBarcodes(string[] barcodes)
        {
            var deviceOrderDtos = await _accessService.GetDirectiveLinesByBarcodes(
                _deviceInfoDto.SystemName,
                barcodes,
                false,
                _deviceInfoDto.LpuId);
            Console.WriteLine($"{DateTime.Now}\t" + $"Получено {deviceOrderDtos.Length} Device Orders для Barcodes=\"{string.Join(", ", barcodes)}\"");

            foreach (var orderDto in deviceOrderDtos)
            {
                var tests = orderDto._directionLines.SelectMany(y =>
                            y._testDTOs.Select(z => z._testId))
                    .Distinct()
                    .Intersect(_testCollationDto.Select(x => x._systemEntityId))
                    .ToArray();

                Console.WriteLine($"{DateTime.Now}\t" + $"\tСопоставлено {tests.Length} показателя для Barcode=\"{orderDto._directionLines.First()._samplebarcode}\"");
                foreach (var test in tests)
                {
                    Console.WriteLine($"{DateTime.Now}\t" + $"\t\tСопоставлено ID=\"{test}\" для Barcode=\"{orderDto._directionLines.First()._samplebarcode}\"");
                }
            }
            return deviceOrderDtos;
        }

        public async Task SetDeviceResults(TestResult[] results)
        {
            var resultsToErrors = new List<TestResult>();

            foreach (var testResults in results)
            {
                var testsForSaveToSend = testResults.Results
                                                    .Where(x =>
                                                        _testCollationDto.Select(y => y._code)
                                                                         .Contains(x.TestCode))
                                                    .ToArray();

                var testForSaveToErrors = testResults.Results
                                             .Except(testsForSaveToSend)
                                             .ToArray();

                if (testForSaveToErrors.Length > 0)
                {
                    var testResultsForErrors = new TestResult()
                    {
                        SampleCode = testResults.SampleCode,
                        Results = testForSaveToErrors
                    };
                    resultsToErrors.Add(testResultsForErrors);
                }

                if (testsForSaveToSend.Length > 0)
                {
                    var deviceOrderDtos = await GetDirectiveLinesByBarcodes(new[] { testResults.SampleCode });

                    foreach (var result in testsForSaveToSend)
                    {
                        Console.WriteLine($"{DateTime.Now}\t" +
                                          $"result.TestCode:{result.TestCode}\t" +
                                          $"result.MuCode:{result.MuCode}");

                        var testCollation = _testCollationDto.SingleOrDefault(x => x._code == result.TestCode)
                                                             ?._systemEntityId;
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
                                            Console.WriteLine($"{DateTime.Now}\t" + $"deviceOrderDto.id:{deviceOrderDto._id}\t" +
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
                                                Console.WriteLine($"{DateTime.Now}\t" + $"Save testCollation:{testCollation}\t" +
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

                                    var directionLines = _accessService.SaveDeviceResults(saveDeviceResults);

                                    // SaveToErrors()
                                    // foreach (var directionLine in directionLines)
                                    // {
                                    //     foreach (var directionLineTest in directionLine._tests)
                                    //     {
                                    //         testsAfterSave.Remove(directionLineTest);
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
                await SaveToErrors(resultsToErrors.ToArray());
                Console.WriteLine($"{DateTime.Now}\t" + $"SaveToErrors");
            }
        }

        private async Task SaveToErrors(TestResult[] testsResultsForErrors)
        {
            var directoryErrors = Path.Combine(Directory.GetCurrentDirectory(), "Errors");
            if (Directory.Exists(directoryErrors) is false)
                Directory.CreateDirectory(directoryErrors);
            
            var errors = $"{directoryErrors}{Path.DirectorySeparatorChar}{_deviceSystemName}.json";

            if (File.Exists(errors))
            {
                var fileTestsResults = await File.ReadAllTextAsync(errors);

                try
                {
                    var afterTestsReults = JsonSerializer.Deserialize<TestResult[]>(fileTestsResults);

                    var testsResultsForConcat = new List<TestResult>();

                    foreach (var testForSave in testsResultsForErrors)
                    {
                        var afterTestResults = afterTestsReults.SingleOrDefault(x => 
                            x.SampleCode == testForSave.SampleCode);

                        if (afterTestResults != default)
                        {
                            var resultsForConcat = new List<Result>();
                            
                            foreach (var resultForSave in testForSave.Results)
                            {
                                var afterTestResult = afterTestResults.Results.SingleOrDefault(x =>
                                    x.TestCode == resultForSave.TestCode);
                                
                                if (afterTestResult != default)
                                {
                                    afterTestResult.Value = resultForSave.Value;
                                    afterTestResult.MuCode = resultForSave.MuCode;
                                }
                                else
                                    resultsForConcat.Add(resultForSave);
                            }

                            afterTestResults.Results = afterTestResults.Results
                                                                       .Concat(resultsForConcat)
                                                                       .ToArray();
                        }
                        else
                            testsResultsForConcat.Add(testForSave);
                    }

                    var toSave = afterTestsReults.Concat(testsResultsForConcat)
                                                 .ToArray();


                    var content = JsonSerializer.Serialize(toSave)
                                                .Replace("[{\"T", "[\r\n\t\t{\"T")
                                                .Replace("[{\"S", "[\r\n\t{\"S")
                                                .Replace("},{\"T", "},\r\n\t\t{\"T")
                                                .Replace("},{\"S", "},\r\n\t{\"S")
                                                .Replace("}]}]", "}]}\r\n]");

                    await File.WriteAllTextAsync(
                        $"{directoryErrors}{Path.DirectorySeparatorChar}{_deviceSystemName}.json",
                        content);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Console.WriteLine($"{DateTime.Now}\t" + exception.Message);
                    File.Delete(errors);

                    var content = JsonSerializer.Serialize(testsResultsForErrors)
                                                .Replace("[{\"T", "[\r\n\t\t{\"T")
                                                .Replace("[{\"S", "[\r\n\t{\"S")
                                                .Replace("},{\"T","},\r\n\t\t{\"T")
                                                .Replace("},{\"S","},\r\n\t{\"S")
                                                .Replace("}]}]","}]}\r\n]");

                    await File.WriteAllTextAsync(   
                        $"{directoryErrors}{Path.DirectorySeparatorChar}{_deviceSystemName}.json",
                        content);
                }
            }
            else
            {
                var content = JsonSerializer.Serialize(testsResultsForErrors)
                                            .Replace("[{\"T", "[\r\n\t\t{\"T")
                                            .Replace("[{\"S", "[\r\n\t{\"S")
                                            .Replace("},{\"T", "},\r\n\t\t{\"T")
                                            .Replace("},{\"S", "},\r\n\t{\"S")
                                            .Replace("}]}]", "}]}\r\n]");

                await File.AppendAllTextAsync(
                    $"{directoryErrors}{Path.DirectorySeparatorChar}{_deviceSystemName}.json",
                    content);
            }
        }
    }
}