using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.TaskService.Entities;

/// <summary>
/// 
/// </summary>
[Table("Comment")]
public class CommentEntity
{
    /// <summary>
    /// 
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public int TaskId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TaskEntity? Task { get; set; }
}