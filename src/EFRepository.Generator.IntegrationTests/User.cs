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
		public DateTimeOffset RegistrationDate { get; set; }
		public DateTimeOffset? TokenExpirationDate { get; set; }
		public bool IsDeleted { get; set; }
		public bool? IsModified { get; set; }
		public double Score { get; set; }
		public double? MaxScore { get; set; }
		public Nullable<long> MinScore { get; set; }

		public virtual ICollection<Post> Posts { get; set; }

		public User()
		{
			Name = string.Empty;
			Phone = string.Empty;
			Address = string.Empty;

			RegistrationDate = DateTimeOffset.MinValue;

			IsModified = null;

			Posts = new List<Post>();
		}
	}
}
