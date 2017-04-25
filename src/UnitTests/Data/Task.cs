using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindfireClientDashboard.Data
{
	public class Task
	{
		//[Key, Column("TaskID")]
		public int TaskID { get; set; }

		public int ProjectID { get; set; }

		public int AssignedUserID { get; set; }

		[Required, MinLength(5), MaxLength(255)]
		public string Name { get; set; }

		public string Description { get; set; }

		public Status Status { get; set; }

		public double OriginalEstimatedTime { get; set; }


		public virtual Project Project { get; set; }

		//[InverseProperty("ID")]
		public virtual User Assigned { get; set; }

		public virtual ICollection<WorkLog> WorkLogs { get; set; }
	}
}
