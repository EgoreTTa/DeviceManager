namespace DeviceManagerAPI
{
    using Controllers.Devices.Services;
    using DeviceManagerService;
    using DeviceManagerService.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddControllers();

            services.AddHostedService<DeviceManagerService>();
            services.AddSingleton<IDevicesControllerService, DevicesControllerService>();
            services.AddSingleton<IDeviceManagerUseService, DeviceManagerUseService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseCors(builder => builder.AllowAnyHeader()
                                          .AllowAnyMethod()
                                          .AllowAnyOrigin());
            app.UseRouting();

            app.UseEndpoints(builder =>
            {
                builder.MapControllers();
            });
        }
    }
}