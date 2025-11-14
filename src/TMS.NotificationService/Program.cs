using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TMS.Common.Extensions;
using TMS.NotificationService.Data.Extensions;
using TMS.NotificationService.Extensions.ApiEndpoints;
using TMS.NotificationService.Extensions.Services;

namespace TMS.NotificationService
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AuthService API",
                    Description = "Minimal API для Сервиса рассылки сообщений."
                });

                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddNotifyDataContext();

            builder.Services.AddFileService("EmailEvents", service =>
            {
                service.BasePath = Environment.GetEnvironmentVariable("BASE_EVENTS_PATH")
                                      ??
                                      throw new InvalidOperationException("BASE_EVENTS_PATH does not defined.");
            });

            builder.Services.AddRabbitMqConsumerConfiguration();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();

            app.Run();
        }
    }
}