namespace DriverTest
{
    using Infrastructure.DTOs.LIS;
    using DriverBase;
    using DriverBase.DTOs;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public sealed class HL7 : IParser
    {
        private readonly StringBuilder _messageForParse = new StringBuilder();
        private readonly Regex _messageRegex = new Regex(@"(?<Message>[\s\S]*\r)\r\n");

        private int _indexFieldForSample = 2;
        private string _nameSegmentForSample = "OBR";
        private int _indexFieldForTestName = 3;
        private int _indexFieldForMeasureUnit = 6;
        private int _indexFieldForValue = 5;

        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public void Clear()
        {
            Logger.Warning("Parser clear...");
            _messageForParse.Clear();
        }

        public OptionDTO[] GetOptions()
        {
            return new[]
            {
                new OptionDTO()
                {
                    Name = nameof(_nameSegmentForSample),
                    Description = "Имя сегмента для поиска номера образца",
                    Value = _nameSegmentForSample,
                    Examples = new[] { "PID", "OBR" },
                },
                new OptionDTO()
                {
                    Name = nameof(_indexFieldForSample),
                    Description = "Номер поля в сегменте для обработки номера образца",
                    Value = _indexFieldForSample,
                    Examples = new[] { "2", "3" },
                },
                new OptionDTO()
                {
                    Name = nameof(_indexFieldForTestName),
                    Description = "Номер поля в OBX-сегменте для обработки имени теста",
                    Value = _indexFieldForTestName,
                    Examples = new[] { "3" },
                },
                new OptionDTO()
                {
                    Name = nameof(_indexFieldForMeasureUnit),
                    Description = "Номер поля в OBX-сегменте для обработки ед.изм. теста",
                    Value = _indexFieldForMeasureUnit,
                    Examples = new[] { "6" },
                },
                new OptionDTO()
                {
                    Name = nameof(_indexFieldForValue),
                    Description = "Номер поля в OBX-сегменте для обработки результата теста",
                    Value = _indexFieldForValue,
                    Examples = new[] { "5" },
                },
            };
        }
       
        public void SetOptions(OptionDTO[] options)
        {
            foreach (var option in options)
            {
                switch (option.Name)
                {
                    case nameof(_indexFieldForSample):
                        _indexFieldForSample = int.Parse($"{option.Value}");
                        break;
                    case nameof(_nameSegmentForSample):
                        _nameSegmentForSample = $"{option.Value}";
                        break;
                    case nameof(_indexFieldForTestName):
                        _indexFieldForTestName = int.Parse($"{option.Value}");
                        break;
                    case nameof(_indexFieldForMeasureUnit):
                        _indexFieldForMeasureUnit = int.Parse($"{option.Value}");
                        break;
                    case nameof(_indexFieldForValue):
                        _indexFieldForValue = int.Parse($"{option.Value}");
                        break;
                }
            }
        }

        public Task<ParserMessage> WriteAsync(byte[] bytes)
        {
            var data = Encoding.GetString(bytes);
            Logger.Debug($" <-:{GetMessageForLogger(data)}");

            var message = new ParserMessage();
            var testResults = new List<TestResultDTO>();

            _messageForParse.Append(data);

            foreach (Match match in _messageRegex.Matches($"{_messageForParse}"))
            {
                Logger.Information($"<--:{GetMessageForLogger($"{match}")}");

                try
                {
                    var acknowledgment = GetAck(match.Groups["Message"].Value);
                    message.ForConnect = Encoding.GetBytes(acknowledgment);
                    Logger.Information($"-->:{GetMessageForLogger(acknowledgment)}");

                    testResults.AddRange(ParseMessage(match.Groups["Message"].Value));
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                    Logger.Debug(exception.StackTrace);
                }

                _messageForParse.Replace(match.Value, string.Empty);

                if (_messageForParse.Length > 0)
                    Logger.Warning($"buffer for message:{GetMessageForLogger(_messageForParse.ToString())}");
            }

            if (testResults.Count > 0)
                message.ForDeviceService = testResults.ToArray();

            return Task.FromResult(message);
        }

        public Task<ParserMessage> ReadAsync() => throw new NotImplementedException();

        public Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders) => throw new NotImplementedException();

        private static string GetMessageForLogger(string message)
        {
            return message.Replace($"\x0B", "<VT>")
                          .Replace($"\x1C", "<FS>")
                          .Replace($"\x0A", "<LF>")
                          .Replace($"\x0D", "<CR>");
        }

        private static string GetAck(string message)
        {
            var segments = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);
            var header = segments.Single(x => x.StartsWith("MSH"));
            var sendingApplication = header.Split('|')[2];
            var sendingFacility = header.Split('|')[3];
            var receivingApplication = header.Split('|')[4];
            var receivingFacility = header.Split('|')[5];
            var messageControlID = header.Split('|')[9];

            return $"\x0B" +
                   $"MSH|^~\\&|{receivingApplication}|{receivingFacility}|{sendingApplication}|{sendingFacility}|{DateTime.Now:yyyyMMddHHmmss}||ACK_R01|{messageControlID}|P|2.3.1||||0||UNICODE\r" +
                   $"MSA|AA|{messageControlID}|Message accepted|||0\r" +
                   $"\x0D\x0A";
        }

        private TestResultDTO[] ParseMessage(string message)
        {
            var testResults = new List<TestResultDTO>();
            var sampleCode = string.Empty;

            var segments = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);

            Logger.Information($"segments {segments.Length} found!");
            foreach (var segment in segments)
            {
                var fields = segment.Split('|');

                if (fields[0] == _nameSegmentForSample) sampleCode = fields[_indexFieldForSample];

                switch (fields[0])
                {
                    case "OBX":
                        var testCode = fields[_indexFieldForTestName];
                        var value = fields[_indexFieldForMeasureUnit];
                        var muCode = fields[_indexFieldForValue];

                        if (string.IsNullOrEmpty(muCode)) muCode = testCode;

                        testResults.Add(new TestResultDTO
                        {
                            SampleCode = sampleCode,
                            TestCode = testCode,
                            Value = value,
                            MuCode = muCode
                        });
                        Logger.Information($"sampleCode:{sampleCode}" +
                                           $"testCode:{testCode}" +
                                           $"value:{value}" +
                                           $"muCode:{muCode}");
                        break;
                }
            }

            return testResults.ToArray();
        }
    }
}