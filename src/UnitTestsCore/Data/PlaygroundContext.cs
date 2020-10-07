using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UnitTestsCore.Data
{
    public partial class PlaygroundContext : DbContext
    {
		public static DbConnection Connection;

        public virtual DbSet<EventLog> EventLog { get; set; }
        public virtual DbSet<TaskAssignments> TaskAssignments { get; set; }
        public virtual DbSet<Tasks> Tasks { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        public PlaygroundContext() : base(new DbContextOptionsBuilder<PlaygroundContext>()
			//.UseInMemoryDatabase("Playground").Options)
			.UseSqlite(CreateInMemoryDatabase()).Options)
		{
			Database.EnsureCreated();
        }

        public PlaygroundContext(DbContextOptions<PlaygroundContext> options)
            : base(options)
        {
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
				//optionsBuilder.UseSqlite(CreateInMemoryDatabase());
            }
        }

		private static DbConnection CreateInMemoryDatabase()
		{
			if (Connection == null)
			{
				Connection = new SqliteConnection("DataSource=file:memdb1?mode=memory&cache=shared");
				Connection.Open();
			}

			return Connection;
		}

		//protected override void OnModelCreating(ModelBuilder modelBuilder)
		//{
		//	//modelBuilder.Entity<EventLog>(entity =>
		//	//{
		//	//	entity.Property(e => e.EventId).ValueGeneratedNever();

		//	//	entity.Property(e => e.EventType).IsFixedLength();
		//	//});

		//	//modelBuilder.Entity<TaskAssignments>(entity =>
		//	//{
		//	//	entity.HasOne(d => d.Task)
		//	//		.WithMany(p => p.TaskAssignments)
		//	//		.HasForeignKey(d => d.TaskId)
		//	//		.OnDelete(DeleteBehavior.ClientSetNull)
		//	//		.HasConstraintName("FK_TaskAssignments_Tasks");

		//	//	entity.HasOne(d => d.User)
		//	//		.WithMany(p => p.TaskAssignments)
		//	//		.HasForeignKey(d => d.UserId)
		//	//		.OnDelete(DeleteBehavior.ClientSetNull)
		//	//		.HasConstraintName("FK_TaskAssignments_Users");
		//	//});

		//	//modelBuilder.Entity<Tasks>(entity =>
		//	//{
		//	//	entity.HasOne(d => d.OwnerUser)
		//	//		.WithMany(p => p.Tasks)
		//	//		.HasForeignKey(d => d.OwnerUserId)
		//	//		.OnDelete(DeleteBehavior.ClientSetNull)
		//	//		.HasConstraintName("FK_Tasks_Users");
		//	//});

		//	//OnModelCreatingPartial(modelBuilder);
		//}

		////partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
