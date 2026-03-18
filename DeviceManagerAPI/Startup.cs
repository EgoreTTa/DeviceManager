namespace DeviceManagerAPI
{
    using DeviceManager.UseCases;
    using DeviceManager.UseCases.UseCaseServices;
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

            services.AddSingleton<IDeviceUseCaseService, DeviceUseCaseService>();
            services.AddSingleton<IDriverUseCaseService, DriverUseCaseService>();
            services.AddSingleton<IDeviceUseCase, DeviceUseCase>();
            services.AddSingleton<IDriverUseCase, DriverUseCase>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseCors(builder => builder.AllowAnyHeader()
                                          .AllowAnyMethod()
                                          .AllowAnyOrigin());

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(builder => { builder.MapControllers(); });
        }
    }
}