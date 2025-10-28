using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.Entities.Task;

[Table("Attachment")]
public class AttachmentEntity
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; }

    [Required]
    [StringLength(255)]
    public string FilePath { get; set; }

    [Required]
    public int TaskId { get; set; }

    public TaskEntity Task { get; set; }
}
