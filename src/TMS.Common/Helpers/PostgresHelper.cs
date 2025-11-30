using Npgsql;
using TMS.Common.Validators;

namespace TMS.Common.Helpers;

public class PostgresHelper
{
    public static string GetConnectionString()
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = Environment.GetEnvironmentVariable("POSTGRES_DB"),
            Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
            Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"),
            CommandTimeout = 30 // таймаут выполнения команд
        };

        var validationResult = PostgreValidator.PostgreSqlConfigurationValidate(builder.Host, builder.Database, builder.Username, builder.Password);
        if (!validationResult.IsValid)
        {
            throw new ArgumentNullException(validationResult.ErrorMessage);
        }

        return builder.ConnectionString;
    }
}
