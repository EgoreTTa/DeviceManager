namespace API
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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