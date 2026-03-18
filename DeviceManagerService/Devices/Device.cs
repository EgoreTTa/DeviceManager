namespace DeviceManager.Devices
{
    using Components;
    using Components.Connect;
    using Components.Service;
    using Configurations.Device;
    using Configurations.Device.Connection;
    using DataAccess.DTOs.LIS;
    using DriverBase;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class Device
    {
        private (Task task, CancellationTokenSource source) _trackTask;
        private IConnection _connection;

        public ILogger Logger { get; set; }
        public DeviceConfig Configuration { get; set; }
        public IParser Parser { get; set; }
        public DeviceService DeviceService { get; set; }
        public DeviceLogs DeviceLogs { get; set; }

        public Device(ILogger logger, DeviceConfig configuration, IParser parser, string url, AppDbContext context, DeviceLogs logs)
        {
            Configuration = configuration;
            Logger = logger;
            if (parser is { })
            {
                Parser = parser;
                parser.Logger = Logger;
            }
            DeviceService = new DeviceService(logger, url, configuration.SystemName, configuration.DriverSystemName, context);
            DeviceLogs = logs;
        }

        public async Task StartAsync()
        {
            if (_trackTask != default) return;

            _connection = Configuration.Connection.ConnectionType switch
            {
                ConnectionTypes.Network => new NetworkConnect(Logger, Configuration.Connection.Network),
                ConnectionTypes.Serial => new SerialConnect(Logger, Configuration.Connection.Serial),
                ConnectionTypes.FileSystem => new FileSystemConnect(Logger, Configuration.Connection.FileSystem),
                _ => throw new ArgumentOutOfRangeException()
            };
            _trackTask.source = new CancellationTokenSource();

            _connection.StartAsync(_trackTask.source.Token);
            DeviceService.GetComparisons();
            _trackTask.task = Run(_trackTask.source.Token);
        }

        private async Task Run(CancellationToken token)
        {
            while (token.IsCancellationRequested is false)
            {
                try
                {
                    var bytesForParser = await _connection.ReadAsync(token);

                    var message = await Parser.WriteAsync(bytesForParser);

                    if (message.ForConnect is { })
                        await _connection.WriteAsync(message.ForConnect, token);

                    if (message.ForDeviceService is { })
                    {
                        var testResults = message.ForDeviceService
                                                 .Where(x =>
                                                     x.SampleCode != string.Empty
                                                     &&
                                                     x.Value != string.Empty)
                                                 .ToArray();
                        await DeviceService.SetDeviceResults(testResults);
                        var barcodes = message.ForDeviceService
                                                 .Where(x =>
                                                     x.SampleCode != string.Empty
                                                     &&
                                                     x.Value == string.Empty)
                                                 .Select(x => x.SampleCode)
                                                 .Distinct()
                                                 .ToArray();
                        if (barcodes.Any())
                        {
                            var orders = await DeviceService.GetDirectiveLinesByBarcodes(barcodes);
                            message = await Parser.WriteAsync(orders);
                            if (message.ForConnect is { })
                                await _connection.WriteAsync(message.ForConnect, token);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                    Logger.Fatal(exception.StackTrace);
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            }
        }

        public async Task StartAsync2(CancellationToken token) // todo
        {
            await _connection.StartAsync(_trackTask.source.Token);
            Parser.Clear();
            while (token.IsCancellationRequested is false)
            {
                try
                {
                    var message = await Parser.ReadAsync();
                    await _connection.WriteAsync(message.ForConnect, token);

                    var bytesForParser = await _connection.ReadAsync(token);
                    message = await Parser.WriteAsync(bytesForParser);
                    await _connection.WriteAsync(message.ForConnect, token);
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            }
        }

        public async Task StopAsync()
        {
            if (_trackTask == default) return;

            _trackTask.source.Cancel();
            await Task.WhenAny(_trackTask.task, Task.Delay(TimeSpan.FromSeconds(5)));
            _trackTask.source.Dispose();

            _trackTask = default;
        }

        public TestCollationDto[] GetTestCollations() => DeviceService.TestCollationDto;

        public async Task RetrySendTestResult(int id)
        {
            await DeviceService.RetrySendTestResult(id);
        }
    }
}