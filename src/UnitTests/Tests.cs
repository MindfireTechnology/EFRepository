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
			TestDataLoader.LoadData(new DashboardDataContext(connection, false));
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
			IRepository userRepo = new Repository(Context);

			var user = userRepo.Query<User>().ByName("Ovaltine", null).ToDto().ToList();


			Assert.IsNotNull(user);
			
			/* FireMap -- Object Mapper -- More Stable
			 * 
			 * var query = await repo.Query<User>()
			 *			.ByUsername()
			 *			.ByFirstName()
			 *			.ByLastName()
			 *			...
			 *			.ByActive()
			 *			.ByRole("Admin")
			 *			.ToListAsync();
			 * 
			 */

			// IQueryable<Task> ByProjectId(this IQueryable<Task> query, params int[] projectIds) // params OR/IN


		}

		[TestMethod]
		public void SimpleInsert()
		{
			IRepository userRepo = new Repository(Context);

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
			IRepository userRepo = new Repository(Context);

			var user = userRepo.Query<User>()
				.Where(n => n.FirstName == "Lavender").FirstOrDefault();
			user.LastName = "Zaugg";

			userRepo.AddOrUpdate(user);
			

			Assert.AreEqual(1, userRepo.Save());

		}

		[TestMethod]
		public void SimpleDelete()
		{
			IRepository userRepo = new Repository(Context);
			var taskRepo = new Repository(Context);
			var workLogRepo = new Repository(Context);

			var user = userRepo.Query<User>().Where(n => n.FirstName == "Bruton").FirstOrDefault();
			userRepo.Delete(user);

			userRepo.Save();

			// Check user is gone
			var deletedUser = userRepo.Query<User>().Where(n => n.FirstName == "Bruton").FirstOrDefault();
			Assert.IsNull(deletedUser);
		}

		[TestMethod]
		public void KeyDelete()
		{
			IRepository userRepo = new Repository(Context);

			userRepo.DeleteOne<User>(10);

			Assert.AreEqual(1, userRepo.Save());
		}


		[TestMethod]
		public void DetachedUpdate()
		{
			IRepository userRepo = new Repository(Context);

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
			IRepository userRepo = new Repository(Context);

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
			IRepository userRepo = new Repository(Context);

			var results = userRepo.Query<User>().Any();

			Assert.IsTrue(results);
		}

		[TestMethod]
		public void SimpleQueryMappedTransaction()
		{
			TinyMapper.Bind<User, UserDTO>();
			IRepository userRepo = new Repository(Context);

			using (var scope = new TransactionScope())
			{

				var user = userRepo.Query<User>().ByName("Schoonie", null).ToDto().ToList();

				Assert.IsNotNull(user);

				scope.Complete();
			}
		}
	}
}
