using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestsCore.Data
{
	[Table("Tasks")]
    public partial class Tasks
    {
        public Tasks()
        {
            TaskAssignments = new HashSet<TaskAssignments>();
        }

        [Key]
        [Column("TaskID")]
        public int TaskId { get; set; }
        [Column("ParentTaskID")]
        public int? ParentTaskId { get; set; }
        [Column("OwnerUserID")]
        public int OwnerUserId { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        [StringLength(4000)]
        public string Description { get; set; }

        [ForeignKey(nameof(OwnerUserId))]
        [InverseProperty(nameof(Users.Tasks))]
        public virtual Users OwnerUser { get; set; }
        [InverseProperty("Task")]
        public virtual ICollection<TaskAssignments> TaskAssignments { get; set; }
    }
}
