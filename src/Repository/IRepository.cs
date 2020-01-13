using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFRepository
{
	/// <summary>
	/// Interface for interacting with data storage through the repository pattern
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IRepository : IDisposable
	{

		/// <summary>Event that fires when an item is added</summary>
		event Action<object> ItemAdded;

		/// <summary>Event that fires when an itemis modified</summary>
		event Action<object> ItemModified;

		/// <summary>Event that fires when an item is deleted</summary>
		event Action<object> ItemDeleted;


		/// <summary>Queriable Entity</summary>
		IQueryable<TEntity> Get<TEntity>() where TEntity : class, new();

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
		/// Add or update entities
		/// </summary>
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
	}
}
