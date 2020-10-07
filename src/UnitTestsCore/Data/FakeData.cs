using System;
using System.Collections.Generic;
using System.Text;
using DataGenerator;
using DataGenerator.Sources;

namespace UnitTestsCore.Data
{
	public class FakeData
	{
		public void PopulateFakeData(PlaygroundContext ctx)
		{
			// Event Log

			Generator.Default.Configure(builder =>
			{
				builder
					.Entity<EventLog>(b =>
					{
						b.AutoMap();
						b.Property(p => p.EventType).DataSource<NameSource>();
						b.Property(p => p.EventData).DataSource<LoremIpsumSource>();
					})
					.Entity<Tasks>(b => b.AutoMap())
					.Entity<TaskAssignments>(b => b.AutoMap())
					.Entity<Users>(b => b.AutoMap());
			});

			var es = Generator.Default.Single<EventLog>();

			ctx.EventLog.AddRange(Generator.Default.List<EventLog>(5));
			ctx.Tasks.AddRange(Generator.Default.List<Tasks>(5));
			ctx.TaskAssignments.AddRange(Generator.Default.List<TaskAssignments>(5));
			ctx.Users.AddRange(Generator.Default.List<Users>(5));

			ctx.SaveChanges();
		}
	}
}
