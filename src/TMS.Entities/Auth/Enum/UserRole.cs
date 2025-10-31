using System.ComponentModel;

namespace TMS.Entities.Auth.Enum;


public enum UserRole
{
    // Базовая роль для всех пользователей
    [Description("Стандартный пользователь")]
    User = 0,

    // Роль для администраторов системы
    [Description("Администратор системы")]
    Admin = 1,

    // Роль для модераторов контента
    [Description("Модератор")]
    Moderator = 2,

    // Роль для супер-администраторов
    [Description("Супер-администратор")]
    SuperAdmin = 3,

    // Роль для гостей (без регистрации)
    [Description("Гость")]
    Guest = 4
}