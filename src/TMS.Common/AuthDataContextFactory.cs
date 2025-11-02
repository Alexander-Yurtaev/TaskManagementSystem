using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TMS.Common;

/// <summary>
/// 
/// </summary>
public class BaseDataContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T CreateDbContext(string[] args)
    {
        // 1. Загружаем .env (если используете)
        DotNetEnv.Env.Load();

        // 2. Получаем строку подключения
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()  // Берём из переменных ОС (включая .env после Env.Load())
            .Build();

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = configuration["POSTGRES_HOST"],
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = configuration["POSTGRES_DB"],
            Username = configuration["POSTGRES_USER"],
            Password = configuration["POSTGRES_PASSWORD"],
            CommandTimeout = 30 // таймаут выполнения команд
        };

        var connectionString = builder.ConnectionString;

        // 3. Настраиваем опции контекста
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(connectionString);

        T instance = (T?)Activator.CreateInstance(typeof(T), new object[] { optionsBuilder.Options })
                     ?? throw new InvalidOperationException($"Не удалось создать экземпляр типа {typeof(T)}");

        return instance!;
    }
}