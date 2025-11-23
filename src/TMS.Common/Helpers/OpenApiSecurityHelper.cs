using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace TMS.Common.Helpers;

public static class OpenApiSecurityHelper
{
    public static void AddSwaggerGenHelper(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        string title,
        string version,
        Action? after = null)
    {
        options.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT токен авторизации (Bearer {token})",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        after?.Invoke();
    }

    public static OpenApiOperation AddSecurityRequirementHelper(OpenApiOperation operation)
    {
        var securities = GetSecurity();
        foreach (var security in securities)
        {
            operation.Security.Add(security);
        }

        return operation;
    }

    #region Private Methods

    private static List<OpenApiSecurityRequirement> GetSecurity()
    {
        var security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "bearer",
                                In = ParameterLocation.Header
                            },
                            Array.Empty<string>()
                        }
                    }
                };
        return security;
    }

    #endregion Private Methods
}
