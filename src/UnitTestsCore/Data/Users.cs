using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestsCore.Data
{
    public partial class Users
    {
        public Users()
        {
            TaskAssignments = new HashSet<TaskAssignments>();
            Tasks = new HashSet<Tasks>();
        }

        [Key]
        [Column("UserID")]
        public int UserId { get; set; }
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        [StringLength(250)]
        public string PassHash { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<TaskAssignments> TaskAssignments { get; set; }
        [InverseProperty("OwnerUser")]
        public virtual ICollection<Tasks> Tasks { get; set; }
    }
}
