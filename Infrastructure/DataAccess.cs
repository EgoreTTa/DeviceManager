namespace Infrastructure
{
    using DTOs.LIS;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    public class DataAccess : IDataAccessService
    {
        private static readonly XNamespace _namespaceSoap = "http://www.bars-open.ru/med/soap/";
        private static readonly XNamespace _envelope = "http://schemas.xmlsoap.org/soap/envelope/";
        private readonly string _httpDevices;
        private readonly string _httpDirectiveLines;
        private readonly string _httpResults;

        private ILogger Logger { get; set; }

        public DataAccess(ILogger logger, string url)
        {
            Logger = logger;
            _httpDevices = $"{url}/devices";
            _httpDirectiveLines = $"{url}/directive_lines";
            _httpResults = $"{url}/results";
        }

        public async Task<DeviceInfoDto> GetDeviceInfo(string driverSystemName)
        {
            Logger.Information($"Запрос устройств использующих драйвер SystemName=\"{driverSystemName}\"");
            var client = new HttpClient();

            var xElements = new[]
            {
                new XElement("request",
                    new XElement("driversystemnameItem", driverSystemName))
            };

            var content = GetRequest("DriverDeviceInfo", xElements);

            Logger.Debug($"Request to {_httpDevices}:\r\n" +
                         $"{content}");
            var responseMessage = await client.PostAsync(new Uri(_httpDevices), new StringContent($"{content}"));

            var responseStringResult = await responseMessage.Content.ReadAsStringAsync();
            var xDocument = XDocument.Parse(responseStringResult);
            Logger.Debug($"Response:\r\n" +
                         $"{xDocument}");
            var method = GetResponse(xDocument);

            var row = method.Element("response")?.Element("data")?.Element("row");

            if (row is { })
            {
                var deviceInfoDto = new DeviceInfoDto
                {
                    DriverSystemName = row.Element("driversystemname")?.Value,
                    Id = row.Element("id")?.Value,
                    IdDriver = row.Element("iddriver")?.Value,
                    WorkMode = row.Element("workmode")?.Value,
                    LpuId = row.Element("lpu")?.Value,
                    Name = row.Element("name")?.Value,
                    Options = row.Element("options")?.Value,
                    SystemName = row.Element("systemname")?.Value,
                    IsActive = row.Element("isactive")?.Value,
                };

                Logger.Information($"Получено устройство Name=\"{deviceInfoDto.Name}\" c SystemName=\"{deviceInfoDto.SystemName}\"");

                return deviceInfoDto;
            }

            Logger.Warning($"Указаный драйвер не зарегистрирован в системе МИС/ЛИС или нет устройств использующий этот драйвер");
            return null;
        }

        public async Task<EnumValueDto[]> GetEnumValues(string driverSystemName)
        {
            Logger.Information($"Запрос перечислимых значений тестов драйвера SystemName=\"{driverSystemName}\"");
            var client = new HttpClient();

            var xElements = new[]
            {
                new XElement("request",
                    new XElement("driversystemnameItem", driverSystemName))
            };

            var content = GetRequest("GetDriversEnumValues", xElements);

            var responseMessage = await client.PostAsync(new Uri(_httpDevices), new StringContent($"{content}"));

            var responseStringResult = await responseMessage.Content.ReadAsStringAsync();
            var xDocument = XDocument.Parse(responseStringResult);

            var method = GetResponse(xDocument);
            var rows = method.Element("response")?.Element("data")?.Elements("row");

            var enumValueDtos = new List<EnumValueDto>();
            foreach (var row in rows)
            {
                enumValueDtos.Add(new EnumValueDto
                {
                    Code = row.Element("code")?.Value,
                    SystemEntityId = row.Element("systementityid")?.Value,
                    DriverSystemName = row.Element("driversystemname")?.Value
                });
            }

            enumValueDtos = enumValueDtos.OrderBy(x => x.Code).ToList();
            Logger.Warning(enumValueDtos.Count > 0
                ? $"Получено \"{enumValueDtos.Count}\" перечислимых значений тестов"
                : "Указаный драйвер не содержит сопоставлений перечислимых значений тестов");
            foreach (var enumValueDto in enumValueDtos)
                Logger.Information($"Перечислимое значение теста=\"{enumValueDto.Code}\" is ID=\"{enumValueDto.SystemEntityId}\"");
            return enumValueDtos.ToArray();
        }

        public async Task<MeasureUnitDto[]> GetMeasureUnits(string driverSystemName)
        {
            Logger.Information($"Запрос единиц измерений драйвера SystemName=\"{driverSystemName}\"");
            var client = new HttpClient();
            var xElements = new[]
            {
                new XElement("request",
                    new XElement("driversystemnameItem", driverSystemName))
            };

            var content = GetRequest("GetDriversMeasureUnits", xElements);

            var responseMessage = await client.PostAsync(new Uri(_httpDevices), new StringContent($"{content}"));

            var responseStringResult = await responseMessage.Content.ReadAsStringAsync();

            var xDocument = XDocument.Parse(responseStringResult);

            var method = GetResponse(xDocument);
            var rows = method.Element("response")?
                .Element("data")?
                .Elements("row");

            var measureUnitDtos = new List<MeasureUnitDto>();
            foreach (var row in rows)
            {
                measureUnitDtos.Add(new MeasureUnitDto
                {
                    Code = row.Element("code")?.Value,
                    SystemEntityId = row.Element("systementityid")?.Value,
                    DriverSystemName = row.Element("driversystemname")?.Value
                });
            }

            measureUnitDtos = measureUnitDtos.OrderBy(x => x.Code).ToList();
            Logger.Information(measureUnitDtos.Count > 0
                ? $"Получено \"{measureUnitDtos.Count}\" единиц измерений"
                : "Указаный драйвер не содержит сопоставлений единиц измерений");
            foreach (var measureUnitDto in measureUnitDtos)
                Logger.Information($"Единица измерения=\"{measureUnitDto.Code}\" is ID=\"{measureUnitDto.SystemEntityId}\"");
            return measureUnitDtos.ToArray();
        }

        public async Task<TestCollationDto[]> GetTestCollations(string driverSystemName)
        {
            Logger.Information($"Запрос результатов исследований драйвера SystemName=\"{driverSystemName}\"");
            using var client = new HttpClient();
            var request = $"<request xmlns=\"\">" +
                          $"    <driversystemnameItem>{driverSystemName}</driversystemnameItem>" +
                          $"</request>";

            var response = await client.PostAsync(
                new Uri(_httpDevices),
                new StringContent(Content("GetDriversTestCollations", request)));

            var responseStringResult = await response.Content.ReadAsStringAsync();
            var xmlDocument = new XmlDocument { InnerXml = responseStringResult };
            var data = xmlDocument.ChildNodes[1]?.FirstChild?.FirstChild?.FirstChild?.FirstChild;
            var testCollations = new List<TestCollationDto>();
            foreach (XmlNode node in data?.ChildNodes)
            {
                testCollations.Add(new TestCollationDto()
                {
                    Code = node["code"]?.InnerText,
                    DeviceId = node["deviceid"]?.InnerText,
                    SystemEntityId = node["systementityid"]?.InnerText,
                    DriverSystemName = node["driversystemname"]?.InnerText,
                    MethodId = node["methodid"]?.InnerText,
                    DeviceSystemName = node["devicesystemname"]?.InnerText,
                });
            }

            testCollations = testCollations.OrderBy(x => x.Code).ToList();
            Logger.Information(testCollations.Count > 0
                                  ? $"Получено \"{testCollations.Count}\" сопоставлений результатов исследований"
                                  : "Указаный драйвер не содержит сопоставлений результатов исследований");
            foreach (var objectResponse in testCollations)
                Logger.Information($"Код теста на приборе=\"{objectResponse.Code}\" is ID=\"{objectResponse.SystemEntityId}\"");
            return testCollations.ToArray();
        }

        public void GetFullDriversTestCollations(string driverSystemName)
        {
            Console.WriteLine($"{DateTime.Now}\t" + $"Запрос данных драйвера SystemName=\"{driverSystemName}\"");

            //             var client = new HttpClient();
            //             var response = client.PostAsync(
            //                     new Uri(_httpDevices),
            //                     new StringContent($"""
            //                                        <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
            //                                        	<s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            //                                        	        xmlns:xsd="http://www.w3.org/2001/XMLSchema">
            //                                        		<GetFullDriversTestCollations xmlns="http://www.bars-open.ru/med/soap/">
            //                                        			<request xmlns="">
            //                                        				<driversystemnameItem>{driverSystemName}</driversystemnameItem>
            //                                        			</request>
            //                                        		</GetFullDriversTestCollations>
            //                                        	</s:Body>
            //                                        </s:Envelope>
            //                                        """))
            //                 .Result;
            //             var responseResult = response.Content.ReadAsStringAsync().Result;
            //             Console.WriteLine(responseResult);
            //             var xmlDocument = new XmlDocument
            //             {
            //                 InnerXml = responseResult
            //             };
            //             var deviceSystemName = xmlDocument.GetElementsByTagName("systemname")
            //                 .Cast<XmlElement>()
            //                 .Select(x => x.InnerText)
            //                 .FirstOrDefault();
            //             if (string.IsNullOrEmpty(deviceSystemName))
            //                 _logger.Information(
            //                     "Указаный драйвер не зарегистрирован в системе МИС/ЛИС или нет устройства использующий этот драйвер");
            //             else
            //                 _logger.Information("DeviceSystemName" + deviceSystemName);
        }

        public async Task<AntibioticDto[]> GetAntibiotics(string driverSystemName)
        {
            Logger.Information($"Запрос антибиотиков драйвера SystemName=\"{driverSystemName}\"");
            using var client = new HttpClient();
            var request = $"<request xmlns=\"\">" +
                          $"    <driversystemnameItem>{driverSystemName}</driversystemnameItem>" +
                          $"</request>";

            var response = await client.PostAsync(
                new Uri(_httpDevices),
                new StringContent(Content("GetDriverAntibiotics", request)));

            var responseStringResult = await response.Content.ReadAsStringAsync();

            var xDocument = XDocument.Parse(responseStringResult);
            var envelope = xDocument.Elements().First();
            var body = envelope.Elements().First();
            var method = body.Elements().First();
            var rows = method.Element("response")?
                .Element("data")?
                .Elements("row");

            var antibioticDtos = new List<AntibioticDto>();
            foreach (var row in rows)
            {
                antibioticDtos.Add(new AntibioticDto
                {
                    _code = row.Element("code").Value,
                    _systemEntityId = row.Element("systementityid").Value,
                    _driverSystemName = row.Element("driversystemname").Value,
                    _antibioticId = row.Element("antibioticid").Value
                });
            }

            antibioticDtos = antibioticDtos.OrderBy(x => x._code).ToList();
            Logger.Information(antibioticDtos.Count > 0
                ? $"Получено \"{antibioticDtos.Count}\" антибиотиков"
                : "Указаный драйвер не содержит сопоставлений антибиотиков");
            foreach (var antibioticDto in antibioticDtos)
                Logger.Information($"Антибиотик=\"{antibioticDto._code}\" is ID=\"{antibioticDto._systemEntityId}\"");
            return antibioticDtos.ToArray();
        }

        public async Task<BacteriumDto[]> GetBacteries(string driverSystemName)
        {
            Logger.Information($"Запрос бактерий драйвера SystemName=\"{driverSystemName}\"");
            using var client = new HttpClient();
            var request = $"<request xmlns=\"\">" +
                          $"    <driversystemnameItem>{driverSystemName}</driversystemnameItem>" +
                          $"</request>";

            var response = await client.PostAsync(
                new Uri(_httpDevices),
                new StringContent(Content("GetDriverBacteries", request)));

            var responseStringResult = await response.Content.ReadAsStringAsync();
            var xmlDocument = new XmlDocument { InnerXml = responseStringResult };
            var data = xmlDocument.ChildNodes[1]?.FirstChild?.FirstChild?.FirstChild?.FirstChild;
            var bacteriumDtos = new List<BacteriumDto>();
            foreach (XmlNode node in data?.ChildNodes)
            {
                bacteriumDtos.Add(new BacteriumDto
                {
                    _code = node["code"].InnerText,
                    _systemEntityId = node["systementityid"].InnerText,
                    _driverSystemName = node["driversystemname"].InnerText,
                    _microorgId = node["microorgid"].InnerText
                });
            }

            bacteriumDtos = bacteriumDtos.OrderBy(x => x._code).ToList();
            Logger.Information(bacteriumDtos.Count > 0
                ? $"Получено \"{bacteriumDtos.Count}\" бактерий"
                : "Указаный драйвер не содержит сопоставлений бактерий");
            foreach (var bacteriumDto in bacteriumDtos)
                Logger.Information($"Бактерия=\"{bacteriumDto._code}\" is ID=\"{bacteriumDto._systemEntityId}\"");
            return bacteriumDtos.ToArray();
        }

        public async Task<BiomaterialDto[]> GetBiomaterials(string driverSystemName)
        {
            Logger.Information($"Запрос биоматериалов драйвера SystemName=\"{driverSystemName}\"");
            using var client = new HttpClient();
            var request = $"<request xmlns=\"\">" +
                          $"    <driversystemnameItem>{driverSystemName}</driversystemnameItem>" +
                          $"</request>";

            var response = await client.PostAsync(
                new Uri(_httpDevices),
                new StringContent(Content("GetDriverBiomaterials", request)));

            var responseStringResult = await response.Content
                                               .ReadAsStringAsync();
            var xmlDocument = new XmlDocument { InnerXml = responseStringResult };
            var data = xmlDocument.ChildNodes[1]?.FirstChild?.FirstChild?.FirstChild?.FirstChild;
            var biomaterialDtos = new List<BiomaterialDto>();
            foreach (XmlNode node in data?.ChildNodes)
            {
                biomaterialDtos.Add(new BiomaterialDto
                {
                    _code = node["code"].InnerText,
                    _systemEntityId = node["systementityid"].InnerText,
                    _driverSystemName = node["driversystemname"].InnerText
                });
            }

            biomaterialDtos = biomaterialDtos.OrderBy(x => x._code).ToList();
            Logger.Information(biomaterialDtos.Count > 0
                ? $"Получено \"{biomaterialDtos.Count}\" биоматериалов"
                : "Указаный драйвер не содержит сопоставлений биоматериалов");
            foreach (var biomaterialDto in biomaterialDtos)
                Logger.Information($"Биоматериал=\"{biomaterialDto._code}\" is ID=\"{biomaterialDto._systemEntityId}\"");
            return biomaterialDtos.ToArray();
        }

        public async Task<DeviceOrderDTO[]> GetDirectiveLinesByBarcodes(
            string deviceSystemName,
            string[] barcodes,
            bool autoSuggestBarcode,
            string lpu)
        {
            Logger.Information($"Запрос строк исследований по Barcodes=\"{string.Join(", ", barcodes)}\"");

            var xElements = new XElement[] { }
                .Append(new XElement(nameof(deviceSystemName).ToLower(), deviceSystemName))
                .Concat(barcodes.Select(x => new XElement(nameof(barcodes), x)))
                .Append(new XElement(nameof(autoSuggestBarcode).ToLower(), autoSuggestBarcode))
                .Append(new XElement(nameof(lpu), lpu))
                .ToArray();

            var content = GetRequest("GetDriverDirectiveLinesByBarcodes", xElements);

            using var client = new HttpClient();

            var response = await client.PostAsync(
                new Uri(_httpDirectiveLines),
                new StringContent(content.ToString()));

            var responseStringResult = await response.Content.ReadAsStringAsync();

            var xDocument = XDocument.Parse(responseStringResult);
            var deviceOrderDTOs = GetResponse(xDocument)
                .Elements("deviceorder")
                .Select(x => new DeviceOrderDTO
                {
                    _id = x.Element("id")?.Value,
                    _deviceSystemName = x.Element("devicesystemname")?.Value,
                    _directionLines = x.Elements("lines")
                        .Select(y => new DirectionLineDTO
                        {
                            _id = y.Element("id")?.Value,
                            _idmethod = y.Element("idmethod")?.Value,
                            _idsample = y.Element("idsample")?.Value,
                            _iddevice = y.Element("iddevice")?.Value,
                            _devicesystemname = y.Element("devicesystemname")?.Value,
                            _samplebarcode = y.Element("samplebarcode")?.Value,
                            _patientage = y.Element("patientage")?.Value,
                            _patientid = y.Element("patientid")?.Value,
                            _patientextid = y.Element("patientextid")?.Value,
                            _patientname1 = y.Element("patientname1")?.Value,
                            _patientname2 = y.Element("patientname2")?.Value,
                            _patientname3 = y.Element("patientname3")?.Value,
                            _patientsex = y.Element("patientsex")?.Value,
                            _patientdepartment = y.Element("patientdepartment")?.Value,
                            _testDTOs = y.Elements("tests")
                                .Select(z => new TestDto
                                {
                                    _resultTypeData = z.Element("resulttypedata")?.Value,
                                    _testId = z.Element("testid")?.Value,
                                    _muId = z.Element("muid")?.Value,
                                    _value = z.Element("value")?.Value,
                                })
                                .ToArray(),
                            _requestedbarcode = y.Element("requestedbarcode")?.Value,
                            _patientbirthdate = y.Element("patientbirthdate")?.Value,
                            _idbiomaterialtype = y.Element("idbiomaterialtype")?.Value,
                            _samplingdatetime = y.Element("samplingdatetime")?.Value,
                            _ordereddate = y.Element("ordereddate")?.Value,
                            _senderinfo = y.Element("senderinfo")?.Value,
                            _senderorganization = y.Element("senderorganization")?.Value,
                            _creatorsharedid = y.Element("creatorsharedid")?.Value,
                            _alternativebarcode = y.Element("alternativebarcode")?.Value,
                        })
                        .ToArray()
                })
                .ToArray();

            return deviceOrderDTOs;
        }

        public async Task<StatusDto[]> SaveDeviceResults(SaveDeviceResultsRequest saveDeviceResultsRequest)
        {
            Logger.Information($"Попытка сохранить результаты исследования");

            var bodyXml = GetBodyXmlRequest(saveDeviceResultsRequest);

            if (bodyXml == null) return null;

            using var client = new HttpClient();

            var response = await client.PostAsync(
                new Uri(_httpResults),
                new StringContent(Content(nameof(SaveDeviceResults), bodyXml)));

            var responseStringResult = await response.Content.ReadAsStringAsync();
            var xmlDocument = new XmlDocument { InnerXml = responseStringResult };
            var data = xmlDocument.ChildNodes[1]?.FirstChild?.FirstChild;
            var statusTestDtos = new List<StatusDto>();
            foreach (XmlNode status in data?.SelectNodes("status"))
            {
                var testDTOs = new List<TestDto>();
                foreach (XmlElement test in status.SelectNodes("tests"))
                {
                    testDTOs.Add(new TestDto
                    {
                        _testId = test["testid"].InnerText
                    });
                }

                statusTestDtos.Add(new StatusDto
                {
                    _id = status["id"].InnerText,
                    _ishiglited = status["ishiglited"].InnerText,
                    _isworktestdatavalid = status["isworktestdatavalid"].InnerText,
                    _status = status["status"].InnerText,
                    _tests = testDTOs.ToArray()
                });
            }

            if (statusTestDtos.Count(x => x._status == "4") > 0)
                Logger.Information($"Обработано \"" +
                                   $"{statusTestDtos.Count(x => x._status == "4")}" +
                                   $"\" строки направления");

            foreach (var statusTestDto in statusTestDtos)
            {
                if (statusTestDto._tests.Length > 0)
                {
                    Logger.Information($"Обработано \"" +
                                       $"{statusTestDto._tests.Length}" +
                                       $"\" тестов строки направления ID=\"" +
                                       $"{statusTestDto._id}\"");
                }
            }

            return statusTestDtos.ToArray();
        }

        private static string Content(string methodName, string request)
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
                   @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""http://www.bars-open.ru/med/soap/"">" +
                   "   <SOAP-ENV:Body>" +
                   $"       <ns1:{methodName}>" +
                   $"           {request}" +
                   $"       </ns1:{methodName}>" +
                   "   </SOAP-ENV:Body>" +
                   "</SOAP-ENV:Envelope>";
        }

        private static XDocument GetRequest(string methodName, XElement[] xElements) =>
            new XDocument(
                new XElement(_envelope + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", _envelope.NamespaceName),
                    new XElement(_envelope + "Body",
                        new XElement(_namespaceSoap + methodName, xElements)
                    )
                )
            );

        private static XElement GetResponse(XDocument xDocument) => xDocument.Elements().First().Elements().First().Elements().First();

        private static string GetBodyXmlRequest<T>(T request)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, request);
            var xmlDocument = new XmlDocument { InnerXml = stringWriter.ToString() };
            return xmlDocument.ChildNodes.Count > 0 ? xmlDocument.ChildNodes[1].InnerXml : null;
        }
    }
}