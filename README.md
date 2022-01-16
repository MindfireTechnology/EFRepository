# EFRepository
Use a LINQ-Enabled version of the Repository Pattern for Entity Framework.

## Features Include
 * Compatible with .NET Framework and .NET Core Frameworks
 * Source Generators automatically create extension methods that allow for a very readable fluent syntax. The generated methods start with `By` E.g.
```C#
var query = Repo.Query<Users>()
	.ByUsernameStartsWith("Bill")
	.ByIsDeleted(false)
	.ByRegistrationDateIsBefore(startDate)
	.AsNoTracking();
```
 * Decoupling Data Context from Code for Better Testability via the `IRepository` interface
 * C# 10 `nullable` compatible code is generated
 * Automatic Update / Insert detection with `.AddOrUpdate`
 * Force Adding of New Object with `.AddNew(object)`
 * FindOne to find objects by key. Will even work with composite keys
 * Events for objects being added or modified or deleted
 * `async` and synchronous methods provided where possible. Chaining to LINQ's async methods is strongly advised (like `.CountAsync()`).

 Note: If a value is passed into a `By` but is null, it will be ignored. If you want to get the the value where the field is null, then use the `By...IsNull()` or `By...IsNotNull()`. E.g.:

 ```C$
var query = Repo.Query<Users>()
	.ByUsernameStartsWith(null); // No users filtered here.
 ```

This is helpful if you want to create a "search" where you have some of the values but always have the option to have some of the values. 

It is often necessary to separate the server-side LINQ statements from the client side. In those cases use the `.AsEnumerable()` function. This will convert the IQueryable to in IEnumerable and everything after that statement will happen after the data is retrieved and there will be no attempt to convert those expressions into the query. A good use-case for this is the mapping of an collection of objects returned from the query to a different type.

## Setting Up
Setting up is easy! Add the `Mindfire.EFRepository` nuget package to your project, then follow the steps below.

### Step 1 - Dependency Registration
In the example below the 
```C#
// This isn't really necessary, but it's a really good idea. If you have a type that you may not use or if 
// you have a type that needs a factory (to use with a using statement, for example), then this pattern 
// might be right up your alley
services.AddTransient(typeof(Lazy<>));
services.AddTransient(typeof(Func<>));

// Now register your DataContext
services.AddScoped<DbContext, ExampleDbContext>();

// Now register IRepository
services.AddScoped<IRepository, Repository>();

// Register your other services that depend on IRepository
services.AddScoped<IExampleService, ExampleService>();
```

### Step 2 - Use In Your Project
This example shows a service that depends on an instance of `IRepository`. This service could then be injected into something like a Controller for API. It is a good idea to map from your EF database objects to domain objects. This controller uses AutoMapper's `IMapper` interface to map between domain objects and data objects.
```C#
public class OrderService : IOrderService
{
	protected IMapper Mapper { get; }
	protected IRepository Repo { get; }

	public OrderService(IMapper mapper, IRepository repository)
	{
		Mapper = mapper;
		Repo = repository;
	}

	public async Task<Order> GetOrder(int orderId)
	{
		var order = await Repo.Query<Order>()
			.ByOrderId(orderId)
			.FirstOrDefaultAsync();

		return Mapper.Map<Order>(order);
	}

	public async Task<Order> AddOrder(Order order)
	{
		var mapped = Mapper.Map<DB.Order>(order);
		mapped.Created = DateTimeOffset.Now;

		Repository.AddOrUpdate(mapped);
		await Repository.SaveAsync();

		return await GetOrder(mapped.OrderId);
	}
	...
}
```

### Step 3 - Unit Testing
Here is a quick example of how to unit test a service that has a dependency on some data from the `IRepository` interface. The example below uses `XUnit` with `Shouldly` and `FakeItEasy`.

You'll need to add a reference to the project with your `DbContext` in it.
```C#
...
using EFRepository;
using FakeItEasy;
using Shouldly;
...
public sealed class OrderServiceTests
{
	[Theory, InlineData(1)]
	public async Task OrderQueryTest(int orderId)
	{
		// Arrange
		var repo = A.Fake<IRepository>();
		A.CallTo(() => repo.Query<Data.Order>())
			.Returns(GetFakeOrderData().AsQueryable());
		var orderService = new OrderService(repo);

		// Act
		var target = await orderService.GetOrderById(orderId);

		// Assert
		A.CallTo(() => repo.Query<Data.Order>()).MustHaveHappened();
		target
			.ShouldNotBeNull()
			.OrderId.ShouldBe(orderId);
	}

	private IEnumerable<Data.Order> GetFakeOrderData() => new[]
	{
		new Data.Order
		{
			OrderId = 1,
			Created = DateTime.Now,
			Email = "homer@compuserv.net"
		}
	};
}
``` 

## Interface
Here is the full interface for `IRepository`. Beyond what is listed in the interface, the source generators create the following prototypes for each of the following types:

### Numeric Types (byte, short, int, long, single, double, decimal) and bool
 * By{Name}({type}? value)
 * By{Name}GreaterThan({type}? value)
 * By{Name}GreaterThanOrEqual({type}? value)
 * By{Name}LessThan({type}? value)
 * By{Name}LessThanOrEqual({type}? value)

### DateTime or DateTimeOffset
 * By{Name}(DateTime? value)
 * By{Name}IsBefore(DateTime? value)
 * By{Name}IsAfter(DateTime? value)
 * By{Name}IsBetween(DateTime? start, DateTime? end)
 * By{Name}OnDate(DateTime? value) - Same day, ignore the time

### String
 * By{Name}(string? value)
 * By{Name}IsNullOrWhiteSpace()
 * By{Name}IsNotNullOrWhiteSpace()
 * By{Name}Contains(string? value)
 * By{Name}StartsWith(string? value)
 * By{Name}EndsWith(string? value)

### Any type that is nullable (including `string`)
 * By{Name}IsNull()
 * By{Name}IsNotNull()

```C#
/// <summary>
/// Interface for interacting with data storage through a queryable repository pattern
/// </summary>
public interface IRepository : IDisposable
{

	/// <summary>Event that fires when an item is added</summary>
	event Action<object> ItemAdding;

	/// <summary>Event that fires when an itemis modified</summary>
	event Action<object> ItemModifing;

	/// <summary>Event that fires when an item is deleted</summary>
	event Action<object> ItemDeleting;


	/// <summary>Queriable Entity</summary>
	IQueryable<TEntity> Query<TEntity>() where TEntity : class, new();

	/// <summary>
	/// Find an entity based on key(s)
	/// </summary>
	/// <param name="keys">The key(s) for the table</param>
	/// <returns>Entity if found, otherwise null</returns>
	TEntity FindOne<TEntity>(params object[] keys) where TEntity : class, new();

	/// <summary>
	/// Find an entity based on key(s)
	/// </summary>
	/// <param name="keys">The key(s) for the table</param>
	/// <returns>Entity if found, otherwise null</returns>
	Task<TEntity> FindOneAsync<TEntity>(params object[] keys) where TEntity : class, new();

	/// <summary>
	/// Adds entities explicily, even if a key is present
	/// </summary>
	/// <param name="values">Entities to add</param>
	void AddNew<TEntity>(params TEntity[] values) where TEntity : class, new();

	/// <summary>
	/// Add or update entities
	/// </summary>
	/// <remarks>
	/// If the key field of the entity is populated with a non-default value, the framework
	/// will assume that the entity is being updated.
	/// </remarks>
	/// <param name="values">Entities to add</param>
	void AddOrUpdate<TEntity>(params TEntity[] values) where TEntity : class, new();

	/// <summary>
	/// Add or update entities
	/// </summary>
	/// <param name="collection">Entities to add</param>
	void AddOrUpdate<TEntity>(IEnumerable<TEntity> collection) where TEntity : class, new();

	/// <summary>
	/// Delete a single entity by key(s)
	/// </summary>
	/// <param name="keys">The key(s) for the table</param>
	void DeleteOne<TEntity>(params object[] keys) where TEntity : class, new();

	/// <summary>
	/// Delete one or more entities
	/// </summary>
	/// <param name="values">Entities to delete</param>
	void Delete<TEntity>(params TEntity[] values) where TEntity : class, new();

	/// <summary>
	/// Delete one or more entities
	/// </summary>
	/// <param name="collection">Entities to delete</param>
	void Delete<TEntity>(IEnumerable<TEntity> collection) where TEntity : class, new();

	/// <summary>
	/// Save pending changes for the collection
	/// </summary>
	/// <returns>Number of affected entities</returns>
	int Save();

	/// <summary>
	/// Save pending changes for the collection async with cancellation
	/// </summary>
	/// <param name="cancellationToken">Cancellation Token</param>
	/// <returns>Number of affected entities</returns>
	Task<int> SaveAsync(CancellationToken cancellationToken = default);
}
```
