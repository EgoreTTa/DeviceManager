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

    public sealed class ASTM : IParser
    {
        private readonly StringBuilder _messageForParse = new StringBuilder();
        private readonly Regex _messageRegex = new Regex(@"(?<Message>H\|[\\\^\&]{3}[\s\S]*?L\|.\|.\r)(|)(?<CheckSum>..)\r\n");

        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public int IndexFieldForSample { get; set; } = 2;
        public string IndexFieldForTestName { get; set; } = "2";
        public int IndexFieldForMeasureUnit { get; set; } = 3;
        public int IndexFieldForValue { get; set; } = 4;

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
                    Name = nameof(IndexFieldForSample),
                    Description = "Номер поля в O-Record для обработки номера образца",
                    Value = IndexFieldForSample,
                    Examples = new[] { "2", "3" },
                },
                new OptionDTO()
                {
                    Name = nameof(IndexFieldForTestName),
                    Description = "Номер поля в R-Record для обработки имени теста",
                    Value = IndexFieldForTestName,
                    Examples = new[] { "2", "2.3", "2.4", "2.5" },
                },
                new OptionDTO()
                {
                    Name = nameof(IndexFieldForMeasureUnit),
                    Description = "Номер поля в R-Record для обработки ед.изм. теста",
                    Value = IndexFieldForMeasureUnit,
                    Examples = new[] { "4" },
                },
                new OptionDTO()
                {
                    Name = nameof(IndexFieldForValue),
                    Description = "Номер поля в R-Record для обработки результата теста",
                    Value = IndexFieldForValue,
                    Examples = new[] { "3", "10" },
                },
            };
        }

        public void SetOptions(OptionDTO[] options)
        {
            foreach (var option in options)
            {
                switch (option.Name)
                {
                    case nameof(IndexFieldForSample):
                        IndexFieldForSample = int.Parse($"{option.Value}");
                        break;
                    case nameof(IndexFieldForTestName):
                        IndexFieldForTestName = $"{option.Value}";
                        break;
                    case nameof(IndexFieldForMeasureUnit):
                        IndexFieldForMeasureUnit = int.Parse($"{option.Value}");
                        break;
                    case nameof(IndexFieldForValue):
                        IndexFieldForValue = int.Parse($"{option.Value}");
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

            switch (bytes.Last())
            {
                case 4:
                    Logger.Information($"<--:<EOT>");
                    break;
                case 5:
                    Logger.Information($"<--:<ENQ>");
                    message.ForConnect = new byte[] { 6 };
                    Logger.Information($"-->:<ACK>");
                    break;
                case 6:
                    Logger.Information($"<--:<ACK>");
                    message.ForConnect = new byte[] { 4 };
                    Logger.Information($"-->:<EOT>");
                    break;
                default:
                    _messageForParse.Append(data);

                    foreach (Match match in _messageRegex.Matches(_messageForParse.ToString()))
                    {
                        Logger.Information($"<--:{GetMessageForLogger(match.ToString())}");

                        message.ForConnect = new byte[] { 6 };
                        Logger.Information($"-->:<ACK>");

                        try
                        {
                            testResults.AddRange(ParseMessage(match.ToString()));
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception.Message);
                            Logger.Debug(exception.StackTrace);
                        }

                        _messageForParse.Replace(match.Value, string.Empty);
                    }

                    if (testResults.Count > 0)
                        message.ForDeviceService = testResults.ToArray();

                    if (_messageForParse.Length > 0)
                        Logger.Warning($"buffer for message:{GetMessageForLogger(_messageForParse.ToString())}");

                    break;
            }

            return Task.FromResult(message);
        }

        public Task<ParserMessage> ReadAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders)
        {
            throw new NotImplementedException();
        }

        private static string GetMessageForLogger(string message)
        {
            return message.Replace($"\x1", "<SOH>")
                          .Replace($"\x2", "<STX>")
                          .Replace($"\x3", "<ETX>")
                          .Replace($"\x4", "<EOT>")
                          .Replace($"\x5", "<ENQ>")
                          .Replace($"\x6", "<ACK>")
                          .Replace($"\x15", "<NAK>")
                          .Replace($"\x17", "<ETB>")
                          .Replace($"\x0A", "<LF>")
                          .Replace($"\x0D", "<CR>");
        }

        private TestResultDTO[] ParseMessage(string message)
        {
            var testResults = new List<TestResultDTO>();
            var sampleCode = string.Empty;

            var records = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);

            foreach (var record in records)
            {
                switch (record[0])
                {
                    case 'O':
                        sampleCode = record.Split('|')[IndexFieldForSample];
                        break;
                    case 'R':
                        var blocks = record.Split('|');
                        var indexesForTestName = IndexFieldForTestName.Split('.');
                        var index = int.Parse(indexesForTestName.First());
                        var testCode = indexesForTestName.Length == 1
                            ? blocks[index]
                            : blocks[index].Split('^')[int.Parse(indexesForTestName.Last())];
                        
                        var value = blocks[IndexFieldForMeasureUnit];
                        var muCode = blocks[IndexFieldForValue];

                        testResults.Add(new TestResultDTO
                        {
                            SampleCode = sampleCode,
                            TestCode = testCode,
                            Value = $"{value}",
                            MuCode = muCode
                        });
                        break;
                }
            }

            return testResults.ToArray();
        }
    }
}