namespace DeviceManager.Entities
{
    using Configurations.Device;
    using Configurations.Device.Connection;
    using DataAccess;
    using DataAccess.DTOs;
    using DriverBase;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Device
    {
        private DataAccess _dataAccess;
        private DeviceInfoDto _deviceInfoDto;
        private TestCollationDto[] _testCollationDto;
        private MeasureUnitDto[] _measureUnitDtos;
        private EnumValueDto[] _enumValueDtos;
        private AntibioticDto[] _antibioticDtos;
        private BacteriumDto[] _bacteriumDtos;
        private BiomaterialDto[] _biomaterialDtos;
        private (Task task, CancellationTokenSource source) _trackTask;
        private Task _taskForRestart;

        public ILogger Logger { get; set; }
        public DeviceConfiguration Configuration { get; set; }
        public IParser Parser { get; set; }
        public AppDbContext DbContext { get; set; }

        public TestCollationDto[] TestCollationDto => _testCollationDto.ToArray();

        public Device(ILogger logger, DeviceConfiguration configuration, IParser parser, DataAccess dataAccess)
        {
            Configuration = configuration;
            Logger = logger;
            Parser = parser;
            _dataAccess = dataAccess;
        }

        public async Task StartAsync()
        {
            if (Configuration.IsActive is false) return;
            _taskForRestart ??= Run();
            _deviceInfoDto = await _dataAccess.GetDeviceInfo(Configuration.DriverConfiguration.SystemName);
            _testCollationDto = await _dataAccess.GetTestCollations(Configuration.DriverConfiguration.SystemName);
            _measureUnitDtos = await _dataAccess.GetMeasureUnits(Configuration.DriverConfiguration.SystemName);
            _enumValueDtos = await _dataAccess.GetEnumValues(Configuration.DriverConfiguration.SystemName);
        }

        private Task Init()
        {
            Parser.Encoding = Encoding.GetEncoding(Configuration.DriverConfiguration.Encoding);
            Parser.Logger = Logger;
            Logger.Information($"Encoding:{Parser.Encoding.BodyName}");
            Logger.Information($"Parser.FullName:{Parser.GetType().FullName}");

            var source = new CancellationTokenSource();
            
            Logger.Information($"ConnectionType:{Configuration.ConnectionConfiguration.ConnectionType}");
            _trackTask = Configuration.ConnectionConfiguration.ConnectionType switch
            {
                ConnectionTypes.Network => (ListenerNetwork(Configuration.ConnectionConfiguration.Network, Parser, source.Token), source),
                ConnectionTypes.Serial => (ListenerSerialPort(Configuration.ConnectionConfiguration.Serial, Parser, source.Token), source),
                ConnectionTypes.FileSystem => (ListenerFileSystem(Configuration.ConnectionConfiguration.FileSystem, Parser, source.Token), source),
                _ => throw new ArgumentOutOfRangeException()
            };
            return Task.CompletedTask;
        }

        private async Task Run()
        {
            do
            {
                try
                {
                    await Init();
                    await Task.WhenAny(_trackTask.task);
                    Logger.Warning($"{Configuration.Name}:{_trackTask.task.Status}... {(_trackTask.task.Status == TaskStatus.Canceled ? "Stop." : "Restart!")}");
                    if (_trackTask.task.Exception is { } testException)  Logger.Error(testException.Message);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                    Logger.Fatal(exception.StackTrace);
                }
            } while (_trackTask.source.IsCancellationRequested is false);

            _taskForRestart = null;
        }

        private async Task ListenerNetwork(NetworkConnection connection, IParser parser, CancellationToken token)
        {
            if (connection.Mode == NetworkModes.Server)
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Loopback, connection.Port);

                using var server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
                server.Bind(ipEndPoint);
                server.Listen(1);

                Logger.Debug($"network {ipEndPoint.Address}:{ipEndPoint.Port} socket server create!");

                var buffer = new byte[2048];
                while (token.IsCancellationRequested is false)
                {
                    var client = await server.AcceptAsync();

                    Logger.Information($"network {client.RemoteEndPoint} socket server connect!");

                    while (client.Connected)
                    {
                        try
                        {
                            var count = await client.ReceiveAsync(buffer, SocketFlags.None, token);
                            var bytes = buffer.Take(count).ToArray();
                            Logger.Debug($"network receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                            parser.Parse(bytes, out var samples, out var send);
                            if (send != null)
                            {
                                await client.SendAsync(send, SocketFlags.None, token);
                                Logger.Debug($"network send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                            }

                            if (samples != null)
                                if (samples.Any(x => x.Results != null))
                                {
                                    // foreach (var testResult in samples) testResult.DeviceId = Configuration.Id;
                                    //
                                    // DbContext.TestResults.AddRange(samples);
                                    // await DbContext.SaveChangesAsync(token);
                                    await SetDeviceResults(samples);
                                }
                                else
                                {
                                    var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                                        Configuration.SystemName,
                                        samples.Select(x => x.SampleCode).ToArray(),
                                        false, //todo flag in configuration?
                                        _deviceInfoDto.LpuId);
                                    parser.ParseOrder(directiveLines, out var order);

                                    await client.SendAsync(order, SocketFlags.None, token);
                                    Logger.Debug($"network send: {string.Join(", ", order.Select(x => $"{x:X2}"))}");
                                }
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception.Message);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }

                        Logger.Warning($"network {client.RemoteEndPoint} end!");
                    }

                    //todo socket.close or socket.disconnect or socket.shutdown?
                }
            }
            else
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Parse(connection.Address), connection.Port);
                using var client = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);

                Logger.Warning($"network {ipEndPoint.Address}:{ipEndPoint.Port} socket client create!");

                var buffer = new byte[2048];
                while (token.IsCancellationRequested is false)
                { 
                    await client.ConnectAsync(ipEndPoint);

                    Logger.Warning($"network {client.RemoteEndPoint} socket client connect!");
                    
                    while (client.Connected)
                    {
                        try
                        {
                            var count = await client.ReceiveAsync(buffer, SocketFlags.None, token);
                            var bytes = buffer.Take(count).ToArray();
                            Logger.Warning($"network receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                            parser.Parse(bytes, out var samples, out var send);
                            if (send != null)
                            {
                                await client.SendAsync(send, SocketFlags.None, token);
                                Logger.Warning($"network send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                            }

                            if (samples != null)
                                if (samples.Any(x => x.Results != null))
                                {
                                    // foreach (var testResult in samples) testResult.DeviceId = Configuration.Id;
                                    //
                                    // DbContext.TestResults.AddRange(samples);
                                    // await DbContext.SaveChangesAsync(token);
                                    // await SetDeviceResults(samples);
                                }
                                else
                                {
                                    var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                                        Configuration.SystemName,
                                        samples.Select(x => x.SampleCode).ToArray(),
                                        false, //todo flag in configuration?
                                        _deviceInfoDto.LpuId);
                                    parser.ParseOrder(directiveLines, out var order);

                                    await client.SendAsync(order, SocketFlags.None, token);
                                    Logger.Debug($"network send: {string.Join(", ", order.Select(x => $"{x:X2}"))}");
                                }
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception.Message);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }

                        Logger.Warning($"network {client.RemoteEndPoint} close!");
                    }

                    //todo socket.close or socket.disconnect or socket.shutdown?
                }
            }
        }

        private async Task ListenerSerialPort(SerialConnection connection, IParser parser, CancellationToken token)
        {
            var serial = new SerialPort($"{connection.PortName}");

            Logger.Debug($"serial {connection.PortName} open...");
            serial.Open();
            Logger.Debug($"serial {connection.PortName} opened.");
            // token.Register(() =>
            // {
            //     Logger.Warning($"serial {connection.PortName} close...");
            //     serial.Close();
            //     Logger.Warning($"serial {connection.PortName} closed!");
            // });
            var count = 0;
            var buffer = new byte[2048];
            while (token.IsCancellationRequested is false)
            {
                try
                {
                    // await Task.Factory.StartNew(() => { count = serial.BaseStream.Read(buffer); }, token);
                    await Task.Run(() => { count = serial.BaseStream.Read(buffer); }, token);
                    // var count = await serial.BaseStream.ReadAsync(buffer, token);
                    var bytes = buffer.Take(count).ToArray();
                    Logger.Debug($"serial {connection.PortName} receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                    Parser.Parse(bytes, out var samples, out var send);
                    if (send != null)
                    {
                        await serial.BaseStream.WriteAsync(send, token);
                        Logger.Debug($"serial {connection.PortName} send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                    }

                    if (samples == null) continue;

                    if (samples.Any(x => x.Results != null))
                    {
                        // foreach (var testResult in samples)
                        // {
                        //     testResult.DeviceId = Configuration.Id;
                        // }
                        //
                        // DbContext.TestResults.AddRange(samples);
                        // await DbContext.SaveChangesAsync(token);
                        await SetDeviceResults(samples);
                    }
                    else
                    {
                        var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                            Configuration.SystemName,
                            samples.Select(x => x.SampleCode).ToArray(),
                            false, //todo flag in configuration?
                            _deviceInfoDto.LpuId);
                        Parser.ParseOrder(directiveLines, out var order);

                        await serial.BaseStream.WriteAsync(order, token);
                        Logger.Debug($"serial {connection.PortName} send: {string.Join(", ", order.Select(x => $"{x:X2}"))}");
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                    Logger.Fatal(exception.StackTrace);
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                    serial.Close();
                    Logger.Debug($"serial {connection.PortName} closed.");
                }
            }
        }

        private async Task ListenerFileSystem(FileSystemConnection connection, IParser parser, CancellationToken token)
        {
            while (token.IsCancellationRequested is false)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                var files = Directory.GetFiles(connection.FolderToRead);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            var bytes = await File.ReadAllBytesAsync(file, token);
                            Logger.Warning($"filesystem {connection.FolderToRead}{Path.DirectorySeparatorChar}{file} receive: {string.Join(", ", bytes.Select(x => $"{x:X2}"))}");

                            parser.Parse(bytes, out var samples, out var send);
                            if (send != null)
                            {
                                await File.WriteAllBytesAsync(connection.FolderToWrite, send, token);
                                Logger.Warning($"filesystem {connection.FolderToWrite} send: {string.Join(", ", send.Select(x => $"{x:X2}"))}");
                            }

                            if (samples != null)
                                if (samples.Any(x => x.Results != null))
                                {
                                    // foreach (var testResult in samples) testResult.DeviceId = Configuration.Id;
                                    //
                                    // DbContext.TestResults.AddRange(samples);
                                    // await DbContext.SaveChangesAsync(token);
                                    // await SetDeviceResults(samples);
                                }
                                else
                                {
                                    var directiveLines = await _dataAccess.GetDirectiveLinesByBarcodes(
                                        Configuration.SystemName,
                                        samples.Select(x => x.SampleCode).ToArray(),
                                        false, //todo flag in configuration?
                                        _deviceInfoDto.LpuId);
                                    parser.ParseOrder(directiveLines, out var order);

                                    await File.WriteAllBytesAsync(connection.FolderToWrite, order, token);
                                    Logger.Debug($"filesystem send: {string.Join(", ", order.Select(x => $"{x:X2}"))}");
                                }
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception.Message);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }
                    }
                }
            }
        }

        private async Task SetDeviceResults(TestResult[] results)
        {
            var resultsToErrors = new List<TestResult>();

            foreach (var testResults in results)
            {
                var testsForSaveToSend = testResults.Results
                                                    .Where(x =>
                                                        _testCollationDto.Select(y => y.Code)
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
                        Logger.Information($"{DateTime.Now}\t" +
                                           $"result.TestCode:{result.TestCode}\t" +
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

                                    var directionLines = _dataAccess.SaveDeviceResults(saveDeviceResults);

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

        public async Task StopAsync(CancellationToken token)
        {
            _trackTask.source.Cancel();
            await Task.WhenAny(_trackTask.task, Task.Delay(TimeSpan.FromSeconds(5), token));
            _trackTask.source.Dispose();
        }
    }
}