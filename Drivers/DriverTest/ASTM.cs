namespace DriverTest
{
    using DataAccess.DTOs;
    using DriverBase;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public sealed class ASTM : IParser
    {
        private readonly StringBuilder _messageForParse = new StringBuilder();
        private readonly StringBuilder _frameForParse = new StringBuilder();
        private readonly Regex _messageRegex = new Regex(@"H\|[\\\^\&]{3}[\s\S]*?\rL\|.\r");
        private readonly Regex _recordRegex = new Regex(@".(?<Record>.*?)(|)(?<CheckSum>..)\r\n");

        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public void Clear()
        {
            Logger.Warning("Parser clear...");
        }

        public void Parse(byte[] bytes, out TestResult[] samples, out byte[] send)
        {
            var data = Encoding.GetString(bytes);
            Logger.Debug($" <-:<{GetMessageForLogger(data)}>");

            samples = null;
            send = null;
            var testResults = new List<TestResult>();

            switch (bytes.Last())
            {
                case 4:
                    Logger.Information($"<--:<EOT>");
                    break;
                case 5:
                    Logger.Information($"<--:<ENQ>");
                    send = new byte[] { 6 };
                    Logger.Information($"-->:<ACK>");
                    break;
                case 6:
                    Logger.Information($"<--:<ACK>");
                    send = new byte[] { 4 };
                    Logger.Information($"-->:<EOT>");
                    break;
                default:
                    _frameForParse.Append(data);

                    if (_recordRegex.IsMatch(_frameForParse.ToString()))
                    {
                        var matchRecord = _recordRegex.Match(_frameForParse.ToString());

                        Logger.Information($"<--:<{GetMessageForLogger(matchRecord.ToString())}>");
                        _messageForParse.Append($"{matchRecord.Groups["Record"].Value}\r");

                        send = new byte[] { 6 };
                        Logger.Information($"-->:<ACK>");

                        _frameForParse.Replace(matchRecord.Value, string.Empty);
                    }

                    if (_frameForParse.Length > 0)
                        Logger.Warning($"buffer for frame:<{GetMessageForLogger(_frameForParse.ToString())}>");


                    foreach (Match match in _messageRegex.Matches(_messageForParse.ToString()))
                    {
                        Logger.Information($"<{GetMessageForLogger(match.ToString())}>");
                        try
                        {
                            testResults.Add(ParseMessage(match.ToString()));
                        }
                        catch (Exception exception)
                        {
                            Logger.Fatal("Parse down", exception);
                            Logger.Error("Parse down", exception);
                        }

                        _messageForParse.Replace(match.Value, string.Empty);
                    }

                    if (testResults.Count > 0) 
                        samples = testResults.ToArray();

                    if (_messageForParse.Length > 0)
                        Logger.Warning($"buffer for message:<{GetMessageForLogger(_messageForParse.ToString())}>");

                    break;
            }
        }

        public void ParseOrder(DeviceOrderDTO[] directiveLines, out byte[] send)
        {
            send = null;
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

        private static TestResult ParseMessage(string message)
        {
            var testResult = new TestResult();
            var results = new List<Result>();

            var records = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);

            foreach (var record in records)
            {
                switch (record[0])
                {
                    case 'P':
                        testResult.SampleCode = record.Split('|')[2];
                        break;
                    case 'R':
                        var blocks = record.Split('|');
                        var testCode = blocks[2].Split('^')[3];
                        var value = blocks[3];
                        var muCode = blocks[4];

                        if (muCode == " "
                            ||
                            string.IsNullOrEmpty(muCode))
                        {
                            muCode = testCode;
                        }

                        switch (testCode)
                        {
                            case "BLD":
                            case "BIL":
                            case "UBG":
                            case "KET":
                            case "GLU":
                            case "PRO":
                            case "NIT":
                            case "LEU":
                                results.Add(new Result
                                {
                                    TestCode = testCode,
                                    Value = $"{testCode}_{value}",
                                    MuCode = muCode
                                });
                                break;
                            case "SG":
                            case "pH":
                                results.Add(new Result
                                {
                                    TestCode = testCode,
                                    Value = $"{value}",
                                    MuCode = muCode
                                });
                                break;
                        }

                        break;
                }
            }

            testResult.Results = results.ToArray();

            return testResult;
        }
    }
}