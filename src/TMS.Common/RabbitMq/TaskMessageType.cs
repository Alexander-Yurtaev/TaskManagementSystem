using System.ComponentModel;

namespace TMS.Common.RabbitMq;

public enum TaskMessageType
{
    [Description("Создание")]
    Create,

    [Description("Редактирование")]
    Update,

    [Description("Удаление")]
    Delete,
}