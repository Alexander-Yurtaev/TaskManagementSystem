using System.ComponentModel;

namespace TMS.AuthService.Entities.Enum;

/// <summary>
/// 
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Роль для гостей (без регистрации)
    /// </summary>
    [Description("Гость")]
    Guest = 0,

    /// <summary>
    /// Базовая роль для всех пользователей
    /// </summary>
    [Description("Стандартный пользователь")]
    User = 1,

    /// <summary>
    /// Роль для администраторов системы
    /// </summary>
    [Description("Администратор системы")]
    Admin = 2,

    /// <summary>
    /// Роль для модераторов контента
    /// </summary>
    [Description("Модератор")]
    Moderator = 3,

    /// <summary>
    /// Роль для супер-администраторов
    /// </summary>
    [Description("Супер-администратор")]
    SuperAdmin = 4
}