# EFRepository
Use a LINQ-Enabled version of the Repository Pattern for Entity Framework

## Interface

	/// <summary>
	/// Interface for interacting with data storage through the repository pattern
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IRepository<TEntity> : IDisposable where TEntity : class, new()
	{
		/// <summary>Queriable Entity</summary>
		IQueryable<TEntity> Entity { get; }

		/// <summary>
		/// Find an entity based on key(s)
		/// </summary>
		/// <param name="keys">The key(s) for the table</param>
		/// <returns>Entity if found, otherwise null</returns>
		TEntity FindOne(params object[] keys);

		/// <summary>
		/// Add or update entities
		/// </summary>
		/// <param name="values">Entities to add</param>
		void AddOrUpdate(params TEntity[] values);

		/// <summary>
		/// Add or update entities
		/// </summary>
		/// <param name="collection">Entities to add</param>
		void AddOrUpdate(IEnumerable<TEntity> collection);

		/// <summary>
		/// Delete a single entity by key(s)
		/// </summary>
		/// <param name="keys">The key(s) for the table</param>
		void DeleteOne(params object[] keys);

		/// <summary>
		/// Delete one or more entities
		/// </summary>
		/// <param name="values">Entities to delete</param>
		void Delete(params TEntity[] values);

		/// <summary>
		/// Delete one or more entities
		/// </summary>
		/// <param name="collection">Entities to delete</param>
		void Delete(IEnumerable<TEntity> collection);

		/// <summary>
		/// Save pending changes for the collection
		/// </summary>
		/// <returns>Number of affected entities</returns>
		int Save();

		/// <summary>
		/// Save pending changes for the collection async
		/// </summary>
		/// <returns>Number of affected entities</returns>
		Task<int> SaveAsync();

		/// <summary>
		/// Save pending changes for the collection async with cancellation
		/// </summary>
		/// <param name="cancellationToken">Cancelation Token</param>
		/// <returns>Number of affected entities</returns>
		Task<int> SaveAsync(CancellationToken cancellationToken);

		/// <summary>Event that fires when an item is added</summary>
		event Action<TEntity> ItemAdded;

		/// <summary>Event that fires when an itemis modified</summary>
		event Action<TEntity> ItemModified;

		/// <summary>Event that fires when an item is deleted</summary>
		event Action<TEntity> ItemDeleted;

## Goals for 2.0
 - Better handling of client-provided ID's
	- What is the expected behavior for AddOrUpdate on a new Client side generated ID?
	- Add an attribute?
 - Better handling of joining of other tables into the query
	- One to One
	- One to Many
	- Many to One
	- Many to Many
 - Better transactional support -- espeically for EF Core
 - Better handling of Child Objects
 - Better support for Transactions
	- Test to see if we're inside of a transaction?
	- If the transaction is already in progress, then throw on BeginTrans
	- Commit or Rollback or throw on dispose?
 - Concurrency
 - AddOrUpdate with a New object that uses an existing object as a relation
	- If you're not using lazy loading, a simple recursive add
	- If you are using lazy loading, then it's your responsibility?
- Soft Deletes
- Better support for Eager, Explicit, and Lazy Loading
