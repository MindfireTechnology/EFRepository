using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindfireClientDashboard.Data
{
	public class WorkLog
	{
		[Key, Column("WorkLogID")]
		public int ID { get; set; }

		public int TaskID { get; set; }

		public int UserID { get; set; }

		[Required]
		public int TotalMinutes { get; set; }

		[Required]
		public string Description { get; set; }

		public virtual Task Task { get; set; }
		public virtual User User { get; set; }
	}
}
