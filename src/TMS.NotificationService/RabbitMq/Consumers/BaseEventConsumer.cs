using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TMS.Common.RabbitMq;
using TMS.NotificationService.Models;
using TMS.NotificationService.Services;

namespace TMS.NotificationService.RabbitMq.Consumers;

/// <summary>
/// 
/// </summary>
/// <param name="serviceScopeFactory"></param>
/// <param name="logger"></param>
/// <param name="queueName"></param>
/// <param name="path"></param>
public abstract partial class BaseEventConsumer(IServiceScopeFactory serviceScopeFactory,
    ILogger<BaseEventConsumer> logger, string queueName, string path)
    : BaseRabbitMqService(logger)
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <exception cref="InvalidOperationException"></exception>
    protected void ProcessMessage(string content)
    {
        try
        {
            // Сохраняем файл с переданным содержимым
            using var scope = serviceScopeFactory.CreateScope();

            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var emailFile = new EmailFile(content, path);
            emailService.Send(emailFile);

            logger.LogInformation($"The email is sent");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fail while send the email: {Message}", ex.Message);
        }
    }
}