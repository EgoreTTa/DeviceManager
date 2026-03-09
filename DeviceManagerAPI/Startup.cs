namespace DeviceManagerAPI
{
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
            services.AddControllers().AddNewtonsoftJson();

            services.AddHostedService<DeviceManagerService>();

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