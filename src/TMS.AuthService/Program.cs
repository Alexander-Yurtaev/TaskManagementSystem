using Microsoft.OpenApi.Models;
using TMS.AuthService.Data.Extensions;
using TMS.AuthService.Extensions.Endpoints;
using TMS.AuthService.Extensions.Services;

namespace TMS.AuthService
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
        /// <exception cref="Exception"></exception>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            using var factory = LoggerFactory.Create(b => b.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            // Add services to the container.
            try
            {
                builder.Services.AddGrpc();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации gRPC.");
                throw;
            }

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Auth Service", Version = "v1" });

                var filePath = Path.Combine(AppContext.BaseDirectory, "APIAuthService.xml");
                c.IncludeXmlComments(filePath);
            });

            // Jwt configurations
            try
            {
                builder.Services.AddJwtAuthentication(builder.Configuration);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации JWT.");
                throw;
            }

            //
            builder.Services.AddAuthorization();

            // Data Context configurations
            try
            {
                builder.Services.AddAuthDataContext();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации AuthDataContext.");
                throw;
            }

            // Перенести в метод AddAuthServices()
            try
            {
                builder.Services.AddAuthServices();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации AuthServices.");
                throw;
            }

            // Redis configurations
            try
            {
                builder.Services.AddRedisConfiguration(builder.Configuration);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации Redis.");
                throw;
            }
            
            //
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGrpcService<Services.Grpc.AuthService>();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();
            app.AddAuthEndpoint();
            app.AddUserEndpoint();

            app.Run();
        }
    }
}
