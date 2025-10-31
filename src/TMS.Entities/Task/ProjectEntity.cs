using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TMS.Entities.Auth;
using TMS.Entities.Task.Enum;

namespace TMS.Entities.Task
{
    [Table("Project")]
    public class ProjectEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [DeniedValues(ProjectStatus.Draft)]
        public ProjectStatus Status { get; set; }

        [Required]
        public int UserId { get; set; }

        [JsonIgnore]
        public UserEntity? User { get; set; }

        public List<TaskEntity> Tasks { get; set; } = [];
    }
}
