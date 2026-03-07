namespace DeviceManagerAPI
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task Main()
        {
            var builder = CreateHostBuilder();

            await builder.Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}