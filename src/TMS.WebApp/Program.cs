using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using TMS.Common.Helpers;
using TMS.Common.Validators;
using TMS.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// автоматически ищет .env в текущей директории
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Настройка Data Protection для сессий и антифоргери токенов
var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
Directory.CreateDirectory(keysDirectory);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetApplicationName("TMS.WebApp")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Поддержка сессий
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// Регистрируем AuthService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();

// Проверка обязательных настроек перед регистрацией сервисов
JwtValidator.ThrowIfNotValidate(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        JwtHelper.ConfigJwt(options, builder.Configuration);
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // 1. Проверяем стандартный заголовок Authorization
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                // 2. Если нет в заголовке, БЕЗОПАСНО проверяем сессию
                if (string.IsNullOrEmpty(token))
                {
                    try
                    {
                        // Проверяем, инициализирована ли сессия
                        token = context.HttpContext.Session?.GetString("access_token");
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Сессия еще не инициализирована - это нормально
                    }
                }

                // 3. Если нет в сессии, проверяем куки
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Cookies["access_token"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Настраиваем HttpClient с перехватчиком
builder.Services.AddHttpClient("AuthenticatedClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
})
.AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<IMigrationService, MigrationService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Добавить в development для детальных ошибок
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

// ВАЖНО: Сессии должны идти ПОСЛЕ UseRouting и ДО UseAuthentication/UseAuthorization
app.UseSession();

app.Use(async (context, next) =>
{
    // Этот middleware гарантирует, что сессия будет загружена до JWT аутентификации
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Обработка исключений (если не в development)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Отладка аутентификации
app.MapGet("/debug/auth", (HttpContext context) =>
{
    var result = new
    {
        // Сессия
        Session = new
        {
            IsAvailable = context.Session?.IsAvailable ?? false,
            Id = context.Session?.Id,
            Keys = context.Session?.Keys.ToArray() ?? Array.Empty<string>(),
            AccessToken = context.Session?.GetString("access_token")
        },

        // Аутентификация
        User = new
        {
            IsAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
            Name = context.User?.Identity?.Name,
            Claims = context.User?.Claims.Select(c => $"{c.Type}: {c.Value}").ToArray()
        },

        // Запрос
        Headers = new
        {
            Authorization = context.Request.Headers["Authorization"].ToString()
        },

        // Куки
        Cookies = new
        {
            AccessToken = context.Request.Cookies["access_token"]
        }
    };

    return Results.Json(result);
});

// Тест логина (симуляция)
app.MapGet("/test/set-token/{token}", (HttpContext context, string token) =>
{
    context.Session.SetString("access_token", token);
    context.Response.Cookies.Append("access_token", token, new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict
    });

    return Results.Ok(new { message = "Token set", token });
});

app.Run();
