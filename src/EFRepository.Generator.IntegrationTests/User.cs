using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFRepository.Generator.IntegrationTests
{
	public record User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string? Phone { get; set; }
		public string? Address { get; set; }
		public DateTime Created { get; set; }
		public bool IsDeleted { get; set; }
		public double Score { get; set; }

		public virtual ICollection<Post> Posts { get; set; }

		public User()
		{
			Name = string.Empty;
			Phone = string.Empty;
			Address = string.Empty;
		}
	}
}
