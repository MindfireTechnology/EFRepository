using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MindfireClientDashboard.Data
{
	public class Project
	{
		[Key, Column("ProjectID")]
		public int ID { get; set; }

		public DateTime CreatedDate { get; set; }

		[Required, MinLength(5), MaxLength(255)]
		public string Name { get; set; }

		public string Description { get; set; }


		public virtual ICollection<Task> Tasks { get; set; }

	}
}
