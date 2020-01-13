using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestsCore.Data
{
    public partial class TaskAssignments
    {
        [Key]
        [Column("TaskAssignmentID")]
        public int TaskAssignmentId { get; set; }
        [Column("UserID")]
        public int UserId { get; set; }
        [Column("TaskID")]
        public int TaskId { get; set; }

        [ForeignKey(nameof(TaskId))]
        [InverseProperty(nameof(Tasks.TaskAssignments))]
        public virtual Tasks Task { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.TaskAssignments))]
        public virtual Users User { get; set; }
    }
}
