using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TMS.Common.Services;

namespace TMS.Common.RabbitMq.Consumers;

public abstract class BaseEventConsumer(IFileService fileService, ILogger<BaseEventConsumer> logger, string queueName, string filePathPrefix)
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
        // Создаём все недостающие папки (включая вложенные)
        var filePath = filePathPrefix; // DirectoryFileHelper.CreateFilePath("BASE_EVENTS_PATH", filePathPrefix, logger);

        if (string.IsNullOrEmpty(filePath)) throw new InvalidOperationException("FilePath is null.");

        // Сохраняем файл с переданным содержимым
        fileService.WriteFile(filePath, (stream) =>
        {
            stream.Write(Encoding.UTF8.GetBytes(content));
        });

        logger.LogInformation($"The file is saved: {filePath}");
    }
}