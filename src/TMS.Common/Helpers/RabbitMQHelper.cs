using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TMS.Common.Validators;

namespace TMS.Common.Helpers;

public static class RabbitMQHelper
{
    public static Tuple<ValidationResult, ConnectionFactory?> CreateConnectionFactory(ILogger logger)
    {
        var rabbitmqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        var rabbitmqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER");
        var rabbitmqPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS");

        var validationResult = RabbitMqValidator.RabbitMqConfigurationValidate(rabbitmqHost, rabbitmqUser, rabbitmqPass);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("RabbitMQ configuration validation failed: {Error}", validationResult.ErrorMessage);
            return new(ValidationResult.Error(validationResult.ErrorMessage!), null);
        }

        var factory = new ConnectionFactory
        {
            HostName = rabbitmqHost!,
            UserName = rabbitmqUser!,
            Password = rabbitmqPass!
        };

        return new (ValidationResult.Success(), factory);
    }
}
