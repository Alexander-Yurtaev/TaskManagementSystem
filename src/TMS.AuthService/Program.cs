using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using TMS.AuthService.Configurations;
using TMS.AuthService.Data;
using TMS.AuthService.Data.Extensions;
using TMS.AuthService.Endpoints;
using TMS.AuthService.Models;
using TMS.AuthService.Services;

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
            // Или более простой вариант:
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Auth Service", Version = "v1" });

                var filePath = Path.Combine(AppContext.BaseDirectory, "APIAuthService.xml");
                c.IncludeXmlComments(filePath);
            });

            // Добавление сервисов аутентификации
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssue = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssue) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new Exception("JWT configuration is not properly set up");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssue,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)
                        )
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddAuthDataContext();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IHashService, HashService>();

            builder.Services.Configure<RedisConfiguration>(builder.Configuration.GetSection("Redis"));
            builder.Services.AddSingleton<ConnectionMultiplexer>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<RedisConfiguration>>().Value;
                return ConnectionMultiplexer.Connect($"{config.Host}:{config.Port}");
            });
            builder.Services.AddTransient<IRedisService<UserToken>, RedisService<UserToken>>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

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
