using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.TaskService.Entities;

/// <summary>
///
/// </summary>
[Table("Attachment")]
public class AttachmentEntity
{
    /// <summary>
    ///
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///
    /// </summary>
    [Required]
    [StringLength(255)]
    public string FilePath { get; set; } = string.Empty;

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