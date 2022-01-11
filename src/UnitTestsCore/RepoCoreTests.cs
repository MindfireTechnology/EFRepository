using System;
using System.Diagnostics;
using System.Linq;
using EFRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UnitTestsCore.Data;
using Xunit;

namespace UnitTestsCore
{
	public class RepoCoreTests
	{
		public static DbContext GetDbContext()
		{
			new FakeData().PopulateFakeData(new PlaygroundContext());
			var ctx = new PlaygroundContext();
			return ctx;
		}

		[Fact]
		public void SimpleInsert()
		{
			var repo = new Repository(GetDbContext(), true);

			var usr = new Users
			{
				Username = "studmuffin",
				PassHash = "What the heck?",
				FirstName = "Felix",
				LastName = "Wreckit"
			};

			repo.AddOrUpdate(usr);
			
			int saved = repo.Save();

			Assert.Equal(1, saved);
		}


		[Fact]
		public void CompoundInsert()
		{
			var repo = new Repository(new PlaygroundContext(), true);

			var task = new Tasks
			{
				Name = "Task 1",
				Description = "A cool task",
				OwnerUser = new Users
				{
					Username = "studmuffin",
					PassHash = "What the heck?",
					FirstName = "Felix",
					LastName = "Wreckit"
				}
			};

			repo.AddOrUpdate(task);

			int saved = repo.Save();

			Assert.Equal(2, saved);
		}


		[Fact]
		public void Delete()
		{
			var repo = new Repository(new PlaygroundContext(), true);

			var usr = new Users
			{
				Username = "studmuffin",
				PassHash = "What the heck?",
				FirstName = "Felix",
				LastName = "Wreckit"
			};

			repo.AddOrUpdate(usr);

			int saved = repo.Save();

			Assert.Equal(1, saved);

			// Now we can delete
			repo.Delete(usr);
			saved = repo.Save();

			Assert.Equal(1, saved);
		}

		[Fact]
		public void DeleteOne()
		{
			var repo = new Repository(new PlaygroundContext(), true);

			var usr = new Users
			{
				Username = "studmuffin",
				PassHash = "What the heck?",
				FirstName = "Felix",
				LastName = "Wreckit"
			};

			repo.AddOrUpdate(usr);

			int saved = repo.Save();

			Assert.Equal(1, saved);

			// Now we can delete
			repo.DeleteOne<Users>(usr.UserId);
			saved = repo.Save();

			Assert.Equal(1, saved);
		}

		[Fact]
		public async void Query()
		{
			var repo = new Repository(new PlaygroundContext(), true);
			string name = Guid.NewGuid().ToString();

			var usr = new Users
			{
				Username = name,
				PassHash = "pwhsh",
				FirstName = "King",
				LastName = "Fallett"
			};

			repo.AddOrUpdate(usr);

			int saved = repo.Save();

			Assert.Equal(1, saved);

			// Now we can Query
			usr = await repo.Query<Users>().Where(n => n.Username == name).SingleOrDefaultAsync();

			Assert.NotNull(usr);
			Assert.Equal(name, usr.Username);
		}

		[Fact]
		public async void Find()
		{
			var repo = new Repository(new PlaygroundContext(), true);

			var usr = new Users
			{
				Username = "studmuffin",
				PassHash = "What the heck?",
				FirstName = "Felix",
				LastName = "Wreckit"
			};

			repo.AddOrUpdate(usr);

			int saved = repo.Save();

			Assert.Equal(1, saved);

			// Now we can Find
			var usr2 = repo.FindOne<Users>(usr.UserId);

			Assert.Equal(usr, usr2);
		}

		[Fact]
		public async void Update()
		{
			var repo = new Repository(new PlaygroundContext(), true);

			var usr = new Users
			{
				Username = "studmuffin",
				PassHash = "What the heck?",
				FirstName = "Felix",
				LastName = "Wreckit"
			};

			repo.AddOrUpdate(usr);

			int saved = repo.Save();

			Assert.Equal(1, saved);

			// Now we can Update
			string name = Guid.NewGuid().ToString();
			usr.FirstName = name;

			saved = await repo.SaveAsync();
			Assert.Equal(1, saved);

			var usr2 = await (from u in repo.Query<Users>()
					   where u.FirstName == name
					   select u).SingleAsync();

			Assert.Equal(usr.FirstName, usr2.FirstName);
		}

		[Fact]
		public async void DisconnectedUpdate()
		{
			string username = Guid.NewGuid().ToString();
			int userid;
			using (var repo = new Repository(new PlaygroundContext(), true))
			{
				var usr = new Users
				{
					Username = username,
					PassHash = "A",
					FirstName = "B",
					LastName = "C"
				};

				repo.AddOrUpdate(usr);
				int saved = await repo.SaveAsync();

				Assert.Equal(1, saved);
				userid = usr.UserId;
			}

			using (var repo = new Repository(new PlaygroundContext(), true))
			{
				var usr = new Users
				{
					UserId = userid,
					Username = username,
					PassHash = "D",
					FirstName = "E",
					LastName = "F"
				};

				repo.AddOrUpdate(usr);
				int saved = await repo.SaveAsync();

				Assert.Equal(1, saved);
			}

			using (var repo = new Repository(new PlaygroundContext(), true))
			{
				var usr = await repo.FindOneAsync<Users>(userid);
				Assert.Equal(userid, usr.UserId);
				Assert.Equal(username, usr.Username);
				Assert.Equal("D", usr.PassHash);
				Assert.Equal("E", usr.FirstName);
				Assert.Equal("F", usr.LastName);
			}
		}

		[Fact]
		public async void MultipleTableQuery()
		{
			// WIP
			using (var repo = new Repository(GetDbContext(), true))
			{
				var result = from u in repo.Query<Users>()
							 from l in u.Tasks
							 where l.Name == "Task 1"
							 select new
							 {
								 u,
								 l,
								 //m = repo.Get<EventLog>()
							 };

				var list = await result.ToArrayAsync();
			}
		}

	}
}
