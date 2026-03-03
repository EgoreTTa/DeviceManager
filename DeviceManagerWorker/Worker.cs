namespace DeviceManagerWorker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Worker
    {
        private static readonly List<IDriver> _drivers = new List<IDriver>() { };
        private static readonly List<Device> _devices = new List<Device>() { };

        private static readonly List<HelpTracking> _helpTracking = new List<HelpTracking>();

        private static Task _tracking;
        private static CancellationTokenSource _trackingTokenSource;

        public static void RunAsync()
        {
            foreach (var device in _devices)
            {
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Device {device.Id} start.");
                // _driverAndTask.Add(driver, driver.StartAsync());

                var ts = new CancellationTokenSource();
                _helpTracking.Add(new HelpTracking()
                {
                    Device = device,
                    TokenSource = ts,
                    Task = device.StartAsync(ts.Token)
                });
            }

            _trackingTokenSource = new CancellationTokenSource();
            _tracking = Tracking(_helpTracking.ToArray(), _trackingTokenSource.Token);
        }

        private static async Task Tracking(HelpTracking[] helpTracking, CancellationToken token)
        {
            try
            {
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Tracking started.");
                // while (Task.WaitAny(driverAndTasks.Values.ToArray(), token) > -1)

                while (token.IsCancellationRequested is false
                       &&
                       helpTracking.Length > 0)
                {
                    Console.WriteLine($"{DateTime.Now}\t" + $"Tracking start...");
                    
                    var index = 0;

                    await Task.Run(
                        () => { index = Task.WaitAny(helpTracking.Select(x => x.Task).ToArray(), token); },
                        token);

                    Console.WriteLine($"{DateTime.Now}\t" + $"Warning!");
                    var tracking = helpTracking[index];
                    switch (tracking.Task.Status)
                    {
                        case TaskStatus.Faulted:
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {helpTracking[index].Device.Name} fail!");
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {tracking.Task.Exception.Message}");
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {tracking.Task.Exception.StackTrace}");
                            break;
                        case TaskStatus.RanToCompletion:
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {helpTracking[index].Device.Name} finish!?");
                            break;
                        case TaskStatus.Canceled:
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {helpTracking[index].Device.Name} cancel!");
                            break;
                    }

                    if (tracking.Task.Status != TaskStatus.Canceled)
                    {
                        tracking.Task = tracking.Device.StartAsync(tracking.TokenSource.Token);
                    }
                   
                    Console.WriteLine($"{DateTime.Now}\t" + $"Tracking end.");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {exception.Message}");
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {exception.StackTrace}");
            }
            finally
            {
                if (helpTracking.Length == 0) Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Tracking empty!");
                if (token.IsCancellationRequested) Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Tracking canceled!");

                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Tracking end.");
            }
        }

        public static async Task AddDevice(Device device)
        {
            _trackingTokenSource.Cancel();

            await _tracking;

            var ts = new CancellationTokenSource();
            _helpTracking.Add(new HelpTracking()
            {
                Device = device,
                TokenSource = ts,
                Task = device.StartAsync(ts.Token)
            });
            _devices.Add(device);
            _drivers.Add(device.Driver);

            _trackingTokenSource = new CancellationTokenSource();
            _tracking = Tracking(_helpTracking.ToArray(), _trackingTokenSource.Token);
        }

        public static async Task RemoveDevice(int id)
        {
            var helpTracking = _helpTracking.Single(x => x.Device.Id == id);
            helpTracking.TokenSource.Cancel();
            await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(5)), helpTracking.Task);
            helpTracking.TokenSource.Dispose();
            
            _trackingTokenSource.Cancel();
            
            await _tracking;
            
            _helpTracking.Remove(helpTracking);
            _devices.Remove(helpTracking.Device);
            _drivers.Remove(helpTracking.Device.Driver);
            
            _trackingTokenSource = new CancellationTokenSource();
            _tracking = Tracking(_helpTracking.ToArray(), _trackingTokenSource.Token);
        }
       
        public static async Task FlipActionDevice(int id) => throw new NotImplementedException();

        public static Task<Device[]> GetDevices()
        {
            return Task.FromResult(_devices.ToArray());
        }
    }
}
