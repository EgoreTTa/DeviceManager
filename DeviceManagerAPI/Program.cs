namespace DeviceManagerAPI
{
    using DeviceManagerWorker;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        private static readonly CancellationToken _token = CancellationToken.None;

        public static async Task Main()
        {
            var builder = CreateHostBuilder();
            Worker.RunAsync();

            await builder.Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
