namespace Core.Devices
{
    using Components;
    using Components.Connect;
    using Components.Service;
    using Core.Configurations.Device;
    using DriverBase;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class Device
    {
        private (Task task, CancellationTokenSource source) _trackTask;

        public DeviceConfig Config { get; set; }
        public IConnection Connection { get; set; }
        public IParser Parser { get; set; }
        public ILogger Logger { get; set; }
        public DeviceService DeviceService { get; set; }
        public DeviceLogs DeviceLogs { get; set; }
        
        public async Task StartAsync()
        {
            if (_trackTask != default) return;

            var source = new CancellationTokenSource();

            Connection?.StartAsync(source.Token);
            DeviceService?.GetComparisons();

            _trackTask = (Run(source.Token), source);
        }

        private async Task Run(CancellationToken token)
        {
            while (token.IsCancellationRequested is false)
            {
                try
                {
                    var bytesForParser = await Connection.ReadAsync(token);

                    var message = await Parser.WriteAsync(bytesForParser);

                    if (message.ForConnect is { })
                        await Connection.WriteAsync(message.ForConnect, token);

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
                                await Connection.WriteAsync(message.ForConnect, token);
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
            await Connection.StartAsync(_trackTask.source.Token);
            Parser.Clear();
            while (token.IsCancellationRequested is false)
            {
                try
                {
                    var message = await Parser.ReadAsync();
                    await Connection.WriteAsync(message.ForConnect, token);

                    var bytesForParser = await Connection.ReadAsync(token);
                    message = await Parser.WriteAsync(bytesForParser);
                    await Connection.WriteAsync(message.ForConnect, token);
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

        public async Task RetrySendTestResult(int id)
        {
            await DeviceService.RetrySendTestResult(id);
        }
    }
}