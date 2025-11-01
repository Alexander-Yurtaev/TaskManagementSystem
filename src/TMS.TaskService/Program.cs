using Microsoft.OpenApi.Models;
using TMS.TaskService.Data.Extensions;
using TMS.TaskService.Extensions.Endpoints;
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

            using var factory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            // gRPC
            builder.Services.AddRpcConfiguration(builder.Configuration);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddTaskDataContext();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Auth Service", Version = "v1" });

                var filePath = Path.Combine(AppContext.BaseDirectory, "APIAuthService.xml");
                c.IncludeXmlComments(filePath);
            });

            // Data Context configurations
            try
            {
                builder.Services.AddTaskDataContext();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "TaskDataContext configuration error");
                throw;
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();
            app.AddTaskServiceOperations();

            app.Run();
        }
    }
}
