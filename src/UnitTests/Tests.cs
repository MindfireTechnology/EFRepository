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
		protected DashboardDataContext Context { get; set; }

		[TestInitialize]
		public void Initialize()
		{
			var connection = Effort.DbConnectionFactory.CreateTransient();
			Context = new DashboardDataContext(connection, true);
			TestDataLoader.LoadData(Context);
		}

		[TestCleanup]
		public void Cleanup()
		{
			Context.Dispose();
		}

		[TestMethod]
		public void SimpleQuery()
		{
			IRepository userRepo = new Repository(Context);

			var results = userRepo.Query<User>()
				.ByUserId(null)
				.ByName("Peter", null).Any();


			Assert.IsTrue(results);
		}

		[TestMethod]
		public void SimpleQueryMapped()
		{
			TinyMapper.Bind<User, UserDTO>();
			IRepository<User> userRepo = new Repository<User>(Context);

			var user = userRepo.Entity.ByName("Ovaltine", null).ToDto().ToList();


			Assert.IsNotNull(user);
		}

		[TestMethod]
		public void SimpleInsert()
		{
			IRepository<User> userRepo = new Repository<User>(Context);

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
			IRepository<User> userRepo = new Repository<User>(Context);

			var user = userRepo.Entity.Where(n => n.FirstName == "Lavender").FirstOrDefault();
			user.LastName = "Zaugg";

			userRepo.AddOrUpdate(user);

			Assert.AreEqual(1, userRepo.Save());
		}

		[TestMethod]
		public void SimpleDelete()
		{
			IRepository<User> userRepo = new Repository<User>(Context);
			var taskRepo = new Repository<Task>(Context);
			var workLogRepo = new Repository<WorkLog>(Context);

			var user = userRepo.Entity.Where(n => n.FirstName == "Bruton").FirstOrDefault();
			userRepo.Delete(user);

			userRepo.Save();

			// Check user is gone
			var deletedUser = userRepo.Entity.Where(n => n.FirstName == "Bruton").FirstOrDefault();
			Assert.IsNull(deletedUser);
		}

		[TestMethod]
		public void KeyDelete()
		{
			IRepository<User> userRepo = new Repository<User>(Context);

			userRepo.DeleteOne(10);

			Assert.AreEqual(1, userRepo.Save());
		}


		[TestMethod]
		public void DetachedUpdate()
		{
			IRepository<User> userRepo = new Repository<User>(Context);

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
			IRepository<User> userRepo = new Repository<User>(Context);

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
			IRepository<Task> userRepo = new Repository<Task>(Context);

			var results = userRepo.Entity.Any();


			Assert.IsTrue(results);
		}

		[TestMethod]
		public void SimpleQueryMappedTransaction()
		{
			TinyMapper.Bind<User, UserDTO>();
			IRepository<User> userRepo = new Repository<User>(Context);

			using (var scope = new TransactionScope())
			{

				var user = userRepo.Entity.ByName("Schoonie", null).ToDto().ToList();

				Assert.IsNotNull(user);

				scope.Complete();
			}
		}
	}
}
