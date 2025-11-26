using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using TMS.Common.Helpers;
using TMS.Common.Validators;
using TMS.TaskService.Data.Extensions;
using TMS.TaskService.Extensions.ApiEndpoints;
using TMS.TaskService.Extensions.ApiEndpoints.Attachments;
using TMS.TaskService.Extensions.ApiEndpoints.Comments;
using TMS.TaskService.Extensions.ApiEndpoints.Projects;
using TMS.TaskService.Extensions.ApiEndpoints.Tasks;
using TMS.TaskService.Extensions.Services;
using TMS.TaskService.Services;

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

            using var factory = LoggerFactory.Create(b => b.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            // автоматически ищет .env в текущей директории
            Env.Load();
            builder.Configuration.AddEnvironmentVariables();

            // Проверка обязательных настроек перед регистрацией сервисов
            JwtValidator.ThrowIfNotValidate(builder.Configuration);

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => JwtHelper.ConfigJwt(options, builder.Configuration));

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => OpenApiSecurityHelper.AddSwaggerGenHelper(options, "Task API", "v1", () =>
            {
                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            }));

            // Регистрация AutoMapper
            builder.Services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());

            // gRPC
            builder.Services.AddRpcConfiguration(builder.Configuration);

            // RabbitMQ
            builder.Services.AddRabbitMqServiceConfiguration();

            builder.Services.AddSingleton<IFileToStorageService, FileToStorageService>();

            builder.Services.AddFileStorageClient();

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

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task API v1");
                    options.RoutePrefix = "swagger"; // URL: /swagger
                });
            }

            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();

            app.AddTasksOperations();
            app.AddProjectsOperations();
            app.AddCommentsOperations();
            app.AddAttachmentsOperations();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            logger.LogInformation("Приложение успешно запущено.");

            app.Run();
        }
    }
}