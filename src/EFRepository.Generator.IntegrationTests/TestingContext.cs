using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFRepository.Generator.IntegrationTests
{
	public class TestingContext : DbContext
	{
		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<Post> Posts { get; set; }

		public TestingContext(DbContextOptions options) : base(options)
		{
		}
	}
}
