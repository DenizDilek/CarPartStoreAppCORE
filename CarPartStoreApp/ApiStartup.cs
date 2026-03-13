using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using CarPartStoreApp.Controllers;

namespace CarPartStoreApp
{
    /// <summary>
    /// ASP.NET Core API Startup Configuration
    /// Configures services, middleware, and HTTP pipeline
    /// </summary>
    public class ApiStartup
    {
        /// <summary>
        /// Configure services and the app request pipeline
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:5173")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });

            // Add controllers
            services.AddControllers();

            // Add API explorer and Swagger for documentation
            services.AddEndpointsApiExplorer();

            // Add Swagger/OpenAPI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Car Parts Storage API",
                    Version = "v1"
                });
            });
        }

        /// <summary>
        /// Configure the HTTP request pipeline
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            // Enable CORS for React development server (localhost:5173)
            app.UseCors();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Enable Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Car Parts Storage API v1");
            });

            // Use routing and map controllers
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
