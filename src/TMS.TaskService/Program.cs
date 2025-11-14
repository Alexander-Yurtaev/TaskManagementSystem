using Microsoft.OpenApi.Models;
using System.Reflection;
using TMS.TaskService.Data.Extensions;
using TMS.TaskService.Extensions.ApiEndpoints;
using TMS.TaskService.Extensions.ApiEndpoints.Attachments;
using TMS.TaskService.Extensions.ApiEndpoints.Comments;
using TMS.TaskService.Extensions.ApiEndpoints.Projects;
using TMS.TaskService.Extensions.ApiEndpoints.Tasks;
using TMS.TaskService.Extensions.Services;

namespace TMS.TaskService
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
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add your features
            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddConsole();
            }

            using var factory = LoggerFactory.Create(b => b.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            builder.Services.AddJwtConfiguration(builder.Environment.ContentRootPath, builder.Configuration, logger);

            // Регистрация AutoMapper
            builder.Services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());

            // gRPC
            builder.Services.AddRpcConfiguration(builder.Configuration);

            // RabbitMQ
            builder.Services.AddRabbitMqServiceConfiguration();

            builder.Services.AddFileStorageClient();

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AuthService API",
                    Description = "Minimal API для Сервиса работы с задачами."
                });

                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            // Data Context configurations
            try
            {
                builder.Services.AddTaskDataContext();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации TaskDataContext.");
                throw;
            }

            try
            {
                builder.Services.AddRepositories();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации TaskServices.");
                throw;
            }

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();

            app.AddTasksOperations();
            app.AddProjectsOperations();
            app.AddCommentsOperations();
            app.AddAttachmentsOperations();

            app.Run();
        }
    }
}