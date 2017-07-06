using MindfireClientDashboard.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests.Data
{
	internal class TestDataLoader
	{
		public static void LoadData(DashboardDataContext context)
		{
			var users = CreateUsers();
			var projects = CreateProjects();
			var tasks = CreateTasks(users, projects);

			int workLogCounter = 1;
			users.ForEach(u =>
			{
				u.Tasks = tasks.Where(t => t.AssignedUserID == u.ID).ToList();

				foreach(var t in u.Tasks)
				{
					t.WorkLogs.Add(CreateWorkLog(u, t, workLogCounter++, $"Work started for {t.Name}.", 10));
					t.WorkLogs.Add(CreateWorkLog(u, t, workLogCounter++, $"Work in progress for {t.Name}.", 30));

					if(t.Status == Status.Waiting || (t.Status == Status.Completed && t.TaskID % 3 == 0))
						t.WorkLogs.Add(CreateWorkLog(u, t, workLogCounter++, $"Work waiting for {t.Name}.", 20));
					if((t.Status == Status.Completed && t.TaskID % 3 == 0))
						t.WorkLogs.Add(CreateWorkLog(u, t, workLogCounter++, $"Work resuming for {t.Name}.", 40));

					if (t.Status == Status.Testing || t.Status == Status.Completed)
						t.WorkLogs.Add(CreateWorkLog(u, t, workLogCounter++, $"Work being tested for {t.Name}.", 25));

					if (t.Status == Status.Completed)
						t.WorkLogs.Add(CreateWorkLog(u, t, workLogCounter++, $"Work finished for {t.Name}.", 60));
				}

				u.WorkLogs = u.Tasks.SelectMany(t => t.WorkLogs).ToList();
			});

			projects.ForEach(p =>
			{
				p.Tasks = tasks.Where(t => t.ProjectID == p.ID).ToList();
			});

			var workLogs = users.SelectMany(u => u.WorkLogs);

			context.Users.AddRange(users);
			context.Projects.AddRange(projects);
			context.Tasks.AddRange(tasks);
			context.WorkLogs.AddRange(workLogs);

			context.SaveChanges();
		}

		protected static List<User> CreateUsers()
		{
			return new List<User>
			{
				new User { ID = 1,  FirstName = "Peter",        LastName = "Panic" },
				new User { ID = 2,  FirstName = "Gus",          LastName = "Jackson" },
				new User { ID = 3,  FirstName = "Francois",     LastName = "" },
				new User { ID = 4,  FirstName = "Magic",        LastName = "Head" },
				new User { ID = 5,  FirstName = "Dr. Mc",       LastName = "Took" },
				new User { ID = 6,  FirstName = "Earnest",      LastName = "Watkins" },
				new User { ID = 7,  FirstName = "Felicia",      LastName = "Fancybottom" },
				new User { ID = 8,  FirstName = "Gus T.",       LastName = "Showbiz" },
				new User { ID = 9,  FirstName = "Ovaltine",     LastName = "Jenkins" },
				new User { ID = 10, FirstName = "Burton",       LastName = "Guster" },
				new User { ID = 11, FirstName = "Shmuel",       LastName = "Cohen" },
				new User { ID = 12, FirstName = "Galileo",      LastName = "Humpkins" },
				new User { ID = 13, FirstName = "Schoonie",     LastName = "Singleton" },
				new User { ID = 14, FirstName = "Nick",         LastName = "Nack" },
				new User { ID = 15, FirstName = "Lavender",     LastName = "Gooms" },
				new User { ID = 16, FirstName = "Bruton",       LastName = "Gaster" },
			};
		}

		protected static List<Project> CreateProjects()
		{
			return Enumerable.Range(1, 5).Select(i => new Project
			{
				ID = i,
				CreatedDate = new DateTime(1970 + (i * 3), (i * 2) % 12, (i * 3) % 30),
				Description = $"Project {i} has started. This is a very important project and should continue.",
				Name = $"Project {i}"
			}).ToList();
		}

		protected static List<Task> CreateTasks(IEnumerable<User> users, IEnumerable<Project> projects)
		{
			return Enumerable.Range(1, 100).Select(i =>
			{
				var user = users.ElementAt(i % users.Count());
				var project = projects.ElementAt(i % projects.Count());

				return new Task
				{
					TaskID = i,
					Assigned = user,
					AssignedUserID = user.ID,
					Project = project,
					ProjectID = project.ID,
					Description = $"Task {i}",
					Name = $"Task {i}",
					OriginalEstimatedTime = i,
					Status = (Status)Enum.GetValues(typeof(Status)).GetValue(i % 4),
					WorkLogs = new List<WorkLog>()
				};
			}).ToList();
		}

		protected static WorkLog CreateWorkLog(User u, Task t, int id, string description, int time)
		{
			return new WorkLog
			{
				ID = id,
				Task = t,
				TaskID = t.TaskID,
				User = u,
				UserID = u.ID,
				Description = description,
				TotalMinutes = time
			};
		}
	}
}
