using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.AuthService.Entities.Enum;

namespace TMS.AuthService.Entities;

/// <summary>
/// 
/// </summary>
[Table("User")]
public class UserEntity
{
    /// <summary>
    /// 
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [StringLength(50)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [DefaultValue(UserRole.User)]
    public UserRole Role { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public DateTime CreateAt { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [DefaultValue(false)]
    public bool IsDeleted { get; set; }
}