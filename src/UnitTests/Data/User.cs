using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindfireClientDashboard.Data
{
	public class User
	{
		[Column("UserID")]
		public int ID { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }


		public virtual ICollection<WorkLog> WorkLogs { get; set; }

		public virtual ICollection<Task> Tasks { get; set; }
	}
}
