using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UnitTestsCore.Data
{
    public partial class PlaygroundContext : DbContext
    {
        public PlaygroundContext()
        {
        }

        public PlaygroundContext(DbContextOptions<PlaygroundContext> options)
            : base(options)
        {
			
        }

        public virtual DbSet<EventLog> EventLog { get; set; }
        public virtual DbSet<TaskAssignments> TaskAssignments { get; set; }
        public virtual DbSet<Tasks> Tasks { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.10.132,2005;User Id=sa;Password=PepperPots1$;Database=Playground");
            }
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<EventLog>(entity =>
        //    {
        //        entity.Property(e => e.EventId).ValueGeneratedNever();

        //        entity.Property(e => e.EventType).IsFixedLength();
        //    });

        //    modelBuilder.Entity<TaskAssignments>(entity =>
        //    {
        //        entity.HasOne(d => d.Task)
        //            .WithMany(p => p.TaskAssignments)
        //            .HasForeignKey(d => d.TaskId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_TaskAssignments_Tasks");

        //        entity.HasOne(d => d.User)
        //            .WithMany(p => p.TaskAssignments)
        //            .HasForeignKey(d => d.UserId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_TaskAssignments_Users");
        //    });

        //    modelBuilder.Entity<Tasks>(entity =>
        //    {
        //        entity.HasOne(d => d.OwnerUser)
        //            .WithMany(p => p.Tasks)
        //            .HasForeignKey(d => d.OwnerUserId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_Tasks_Users");
        //    });

        //    OnModelCreatingPartial(modelBuilder);
        //}

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
