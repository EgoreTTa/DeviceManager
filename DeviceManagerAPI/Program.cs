namespace DeviceManagerAPI
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task Main()
        {
            var builder = CreateHostBuilder()
                .ConfigureLogging(logBuilder =>
                {
                    // logBuilder.ClearProviders();
                    // logBuilder.AddProvider(new FileLoggerProvider("Logs/DeviceManager/Log.txt"));
                    // logBuilder.SetMinimumLevel(LogLevel.Trace);
                });

            await builder.Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    // public class FileLoggerProvider : ILoggerProvider
    // {
    //     private readonly ILogger _logger;
    //
    //     public FileLoggerProvider(string filePath)
    //     {
    //         _logger = new FileLogger(filePath);
    //     }
    //
    //     public void Dispose() { }
    //     public ILogger CreateLogger(string categoryName) => _logger;
    // }
    //
    // public class FileLogger : ILogger, IDisposable
    // {
    //     private readonly string _filePath;
    //
    //     public FileLogger(string filePath)
    //     {
    //         _filePath = filePath;
    //         WriteLog($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] log start...");
    //     }
    //
    //     public IDisposable BeginScope<TState>(TState state) => this;
    //     public bool IsEnabled(LogLevel logLevel) => true;
    //
    //     public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    //     {
    //         var fileInfo = new FileInfo(_filePath);
    //         if (fileInfo.Length / 1024 / 1024 > 1) // 1 MB
    //             fileInfo.MoveTo($"Logs/DeviceManager/Log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
    //
    //         WriteLog($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logLevel}:{state}{exception}");
    //     }
    //
    //     private void WriteLog(string content)
    //     {
    //         Directory.CreateDirectory("Logs/DeviceManager/");
    //         File.AppendAllText(_filePath, $"{content}{Environment.NewLine}");
    //     }
    //
    //     public void Dispose() { }
    //
    //     ~FileLogger()
    //     {
    //         WriteLog($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] log end.");
    //     }
    // }
}
