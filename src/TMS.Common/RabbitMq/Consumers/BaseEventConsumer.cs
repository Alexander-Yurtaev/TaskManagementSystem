using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TMS.Common.RabbitMq.Consumers;

public abstract class BaseEventConsumer(ILogger<BaseEventConsumer> logger, string queueName, string filePathPrefix)
    : BaseRabbitMqService(logger)
{
    public override async Task<string> InitializeAsync()
    {
        await base.InitializeAsync();

        if (Channel is null)
        {
            logger.LogError("Channel is not defined.");
            throw new NullReferenceException("Channel is not defined.");
        }

        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            ProcessMessage(message);

            await Channel.BasicAckAsync(ea.DeliveryTag, false);
        };
        // this consumer tag identifies the subscription
        // when it has to be cancelled
        return await Channel.BasicConsumeAsync(queueName, false, consumer);
    }

    protected void ProcessMessage(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePathPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePathPrefix);

        var eventsPath = Environment.GetEnvironmentVariable("EVENTS_PATH");
        if (string.IsNullOrEmpty(eventsPath))
        {
            throw new Exception("EVENTS_PATH is not defined.");
        }

        var filePath = Path.Combine(eventsPath, filePathPrefix);

        // 1. Получаем каталог из полного пути к файлу
        string? directoryPath = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(directoryPath)) throw new NullReferenceException("DirectoryPath is null.");

        // 2. Создаём все недостающие папки (включая вложенные)
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Console.WriteLine($"Создана структура папок: {directoryPath}");
        }

        // 3. Сохраняем файл с переданным содержимым
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Файл сохранён: {filePath}");
    }
}