namespace DeviceManagerAPI
{
    using Controllers.Devices.Services;
    using Controllers.Drivers.Services;
    using DeviceManager;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();

            services.AddHostedService<DeviceManagerService>();

            services.AddSingleton<IDriversControllerService, DriversControllerService>();
            services.AddSingleton<IDevicesControllerService, DevicesControllerService>();
            services.AddSingleton<IDeviceManager, DeviceManager>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder.AllowAnyHeader()
                                          .AllowAnyMethod()
                                          .AllowAnyOrigin());
            app.UseRouting();

            app.UseEndpoints(builder => { builder.MapControllers(); });
        }
    }
}