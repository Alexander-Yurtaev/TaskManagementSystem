using Microsoft.OpenApi.Models;
using System.Reflection;
using TMS.Common.Extensions;
using TMS.FileStorageService.Extensions.ApiEndpoints;

namespace TMS.FileStorageService
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

            #region Logging

            using var factory = LoggerFactory.Create(b => b.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            #endregion Logging

            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddConsole();
            }

            // Add services to the container.
            builder.Services.AddFileService("AttachmentFiles", options =>
            {
                options.BasePath = Environment.GetEnvironmentVariable("BASE_FILES_PATH")
                                      ??
                                      throw new InvalidOperationException("BASE_FILES_PATH does not defined.");
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AuthService API",
                    Description = "Minimal API для Сервиса работы с файлами."
                });

                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

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
            app.AddFileStoragesEndpoint();

            app.Run();
        }
    }
}