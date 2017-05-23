using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindfireClientDashboard.Data;
using EFRepository;
using System.Linq;
using UnitTests.Data;
using Nelibur.ObjectMapper;
using UnitTests.Serialized;
using System.Transactions;

namespace UnitTests
{
	[TestClass]
	public class Tests
	{
		[TestMethod]
		public void SimpleQuery()
		{
			var context = new DashboardDataContext();
			IRepository<User> userRepo = new Repository<User>(context);

			var results = userRepo.Entity
				.ByUserId(null)
				.ByName("Nate", null).Any();


			Assert.IsTrue(results);
		}

		[TestMethod]
		public void SimpleQueryMapped()
		{
			var context = new DashboardDataContext();
			TinyMapper.Bind<User, UserDTO>();
			IRepository<User> userRepo = new Repository<User>(context);

			var user = userRepo.Entity.ByName("Nate", null).ToDto().ToList();


			Assert.IsNotNull(user);
		}

		[TestMethod]
		public void SimpleInsert()
		{
			var context = new DashboardDataContext();
			IRepository<User> userRepo = new Repository<User>(context);

			userRepo.AddOrUpdate(new User
			{
				FirstName = "Nate",
				LastName = "Zaugg",
			});

			Assert.AreEqual(1, userRepo.Save());
		}

		[TestMethod]
		public void SimpleUpdate()
		{
			var context = new DashboardDataContext();
			IRepository<User> userRepo = new Repository<User>(context);

			var user = userRepo.Entity.Where(n => n.FirstName == "Nate").FirstOrDefault();
			user.LastName = "Zaugg";

			userRepo.AddOrUpdate(user);

			userRepo.Save();
		}

		[TestMethod]
		public void SimpleDelete()
		{
			var context = new DashboardDataContext();
			IRepository<User> userRepo = new Repository<User>(context);

			var user = userRepo.Entity.Where(n => n.FirstName == "Nate").FirstOrDefault();
			userRepo.Delete(user);

			userRepo.Save();
		}

		[TestMethod]
		public void KeyDelete()
		{
			var context = new DashboardDataContext();
			IRepository<User> userRepo = new Repository<User>(context);

			userRepo.Delete(10);

			userRepo.Save();
		}


		[TestMethod]
		public void DetachedUpdate()
		{
			var context = new DashboardDataContext();
			IRepository<User> userRepo = new Repository<User>(context);

			var user = new User
			{
				ID = 4,
				FirstName = "Nate-Updated",
				LastName = "Zaugg-Updated",
			};

			userRepo.AddOrUpdate(user);

			Assert.AreEqual(1, userRepo.Save());
		}

		[TestMethod]
		public void DetachedUpdateWithExistingEntity()
		{
			var context = new DashboardDataContext();
			IRepository<User> userRepo = new Repository<User>(context);

			userRepo.Entity.ToList(); // Load everything!
			
			var user = new User
			{
				ID = 4,
				FirstName = "Nate-Updated",
				LastName = "Zaugg-Updated",
			};

			userRepo.AddOrUpdate(user);

			Assert.AreEqual(1, userRepo.Save());
		}



		[TestMethod]
		public void SimpleIDKeyAttributeMissingQuery()
		{
			var context = new DashboardDataContext();
			IRepository<Task> userRepo = new Repository<Task>(context);

			var results = userRepo.Entity.Any();


			Assert.IsTrue(results);
		}

		[TestMethod]
		public void SimpleQueryMappedTransaction()
		{
			var context = new DashboardDataContext();
			TinyMapper.Bind<User, UserDTO>();
			IRepository<User> userRepo = new Repository<User>(context);

			using (var scope = new TransactionScope())
			{

				var user = userRepo.Entity.ByName("Nate", null).ToDto().ToList();

				Assert.IsNotNull(user);

				scope.Complete();
			}
		}
	}
}
