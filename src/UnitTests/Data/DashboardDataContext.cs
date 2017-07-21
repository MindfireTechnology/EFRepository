using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindfireClientDashboard.Data
{
	public class DashboardDataContext : DbContext
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<Task> Tasks { get; set; }
		public DbSet<WorkLog> WorkLogs { get; set; }

		public DashboardDataContext() : base()
		{
		}

		public DashboardDataContext(DbConnection connection, bool ownsConnection) : base(connection, ownsConnection)
		{
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>()
			.HasMany(u => u.Tasks)
			.WithRequired(t => t.Assigned)
			.WillCascadeOnDelete(true);

			modelBuilder.Entity<User>()
			.HasMany(u => u.WorkLogs)
			.WithRequired(t => t.User)
			.WillCascadeOnDelete(true);

			modelBuilder.Entity<Task>()
			.HasMany(t => t.WorkLogs)
			.WithRequired(w => w.Task)
			.WillCascadeOnDelete(true);

			modelBuilder.Entity<Project>()
			.HasMany(p => p.Tasks)
			.WithRequired(t => t.Project)
			.WillCascadeOnDelete(true);
		}
	}
}
