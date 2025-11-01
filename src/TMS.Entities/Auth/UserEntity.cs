using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Entities.Auth.Enum;
using TMS.Entities.Task;

namespace TMS.Entities.Auth;

[Table("User")]
public class UserEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [DefaultValue(UserRole.User)]
    public UserRole Role { get; set; }

    [Required]
    public DateTime CreateAt { get; init; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    [DefaultValue(false)]
    public bool IsDeleted { get; set; }

    public List<CommentEntity> Comments { get; set; } = [];

    public List<ProjectEntity> Projects { get; set; } = [];
}