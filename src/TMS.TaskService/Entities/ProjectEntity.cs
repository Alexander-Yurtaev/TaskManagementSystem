using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TMS.TaskService.Entities.Enum;

namespace TMS.TaskService.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("Project")]
    public class ProjectEntity
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
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [DeniedValues(ProjectStatus.Draft)]
        public ProjectStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<TaskEntity> Tasks { get; set; } = [];
    }
}
