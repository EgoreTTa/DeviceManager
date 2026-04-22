namespace BeckmanCoulter
{
    using DriverBase;
    using DriverBase.DTOs;
    using Infrastructure.DTOs.LIS;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class DxH500 : IParser
    {
        private readonly Regex _regexMatchFrame = new Regex(@".(?<Body>[\s\S]*?)[](?<Checksum>..)\r\n");
        private readonly Regex _regexMatchMessage = new Regex(@"H\|[\s\S]*?\rL\|\d\|.\r");
        private readonly StringBuilder _bufferOfFrame = new StringBuilder();
        private readonly StringBuilder _bufferOfMessage = new StringBuilder();

        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; }

        public void Clear()
        {
            _bufferOfFrame.Clear();
            _bufferOfMessage.Clear();
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
                    SendACK(message);
                    break;
                case 6:
                    Logger.Information($"<--:<ACK>");
                    SendEOT(message);
                    break;
                default:
                    _bufferOfFrame.Append(data);

                    foreach (Match match in _regexMatchFrame.Matches($"{_bufferOfFrame}"))
                    {
                        Logger.Information($"<--:{GetMessageForLogger($"{match}")}");
                        if (match.Groups["Checksum"].Value != $"{CalculateChecksum(match.Groups["Body"].Value):X2}")
                            SendNAK(message);

                        SendACK(message);

                        _bufferOfMessage.Append(match.Groups["Body"].Value);

                        _bufferOfFrame.Replace(match.Value, string.Empty);
                        if (_bufferOfFrame.Length > 0)
                            Logger.Warning($"buffer of frame:{GetMessageForLogger($"{_bufferOfFrame}")}");
                    }

                    foreach (Match match in _regexMatchMessage.Matches($"{_bufferOfMessage}"))
                    {
                        Logger.Information($"{GetMessageForLogger($"{match}")}");

                        _bufferOfMessage.Append(match.Value);

                        try
                        {
                            testResults.AddRange(ParseMessage($"{match}"));
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception.Message);
                            Logger.Debug(exception.StackTrace);
                        }

                        _bufferOfMessage.Replace(match.Value, string.Empty);
                        if (_bufferOfMessage.Length > 0)
                            Logger.Warning($"buffer of message:{GetMessageForLogger($"{_bufferOfMessage}")}");
                    }

                    if (testResults.Count > 0)
                        message.ForDeviceService = testResults.ToArray();

                    break;
            }

            return Task.FromResult(message);
        }

        public Task<ParserMessage> ReadAsync() => throw new NotImplementedException();

        public Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders) => throw new NotImplementedException();

        private static TestResultDTO[] ParseMessage(string message)
        {
            var testResults = new List<TestResultDTO>();
            var sampleCode = string.Empty;

            var records = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);

            foreach (var record in records)
            {
                switch (record[0])
                {
                    case 'O':
                        sampleCode = record.Split('|')[2].Split('!')[0];
                        break;
                    case 'R':
                        var blocks = record.Split('|');
                        var testCode = blocks[2].Split('!')[3];
                        var value = blocks[3].Split('!')[0].Trim();
                        var muCode = blocks[4];

                        testResults.Add(new TestResultDTO
                        {
                            SampleCode = sampleCode,
                            TestCode = testCode,
                            Value = value,
                            MuCode = muCode
                        });
                        break;
                }
            }

            return testResults.ToArray();
        }

        private void SendEOT(ParserMessage message)
        {
            message.ForConnect = new byte[] { 4 };
            Logger.Information($"-->:<EOT>");
        }

        private void SendACK(ParserMessage message)
        {
            message.ForConnect = new byte[] { 6 };
            Logger.Information($"-->:<ACK>");
        }

        private void SendNAK(ParserMessage message)
        {
            message.ForConnect = new byte[] { 21 };
            Logger.Information($"-->:<NAK>");
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

        private static byte CalculateChecksum(string bodyMessage)
        {
            byte res = 0;
            foreach (byte symbol in bodyMessage)
                if (symbol > 2) res += symbol;

            return res;
        }
    }
}
