using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Entities.Auth;

namespace TMS.Entities.Task;

[Table("Comment")]
public class CommentEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public int UserId { get; set; }

    public UserEntity User { get; set; }

    [Required]
    public int TaskId { get; set; }

    public TaskEntity Task { get; set; }
}