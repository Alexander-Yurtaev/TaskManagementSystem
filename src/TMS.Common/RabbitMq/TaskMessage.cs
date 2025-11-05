namespace TMS.Common.RabbitMq;

public record TaskMessage(TaskMessageType Type, string Message);