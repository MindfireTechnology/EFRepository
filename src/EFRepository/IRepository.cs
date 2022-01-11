using System;
using System.Collections.Generic;
using System.Data;
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
		event Action<object> ItemAdding;

		/// <summary>Event that fires when an itemis modified</summary>
		event Action<object> ItemModifing;

		/// <summary>Event that fires when an item is deleted</summary>
		event Action<object> ItemDeleting;


		/// <summary>Queriable Entity</summary>
		IQueryable<TEntity> Query<TEntity>() where TEntity : class, new();

		/// <summary>
		/// Join another entity
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		IQueryable<TEntity> Join<TEntity>() where TEntity : class, new();

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
		/// Save pending changes for the collection async
		/// </summary>
		/// <returns>Number of affected entities</returns>
		Task<int> SaveAsync();

		/// <summary>
		/// Save pending changes for the collection async with cancellation
		/// </summary>
		/// <param name="cancellationToken">Cancelation Token</param>
		/// <returns>Number of affected entities</returns>
		Task<int> SaveAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Begins a transaction at the specified isolation level
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an excpetion if there is already a transaction in progress</exception>
		/// <param name="isolation">The desired transaction isolation level</param>
		void StartTransaction(IsolationLevel isolation);

		/// <summary>
		/// Begins a transaction at the specified isolation level
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an excpetion if there is already a transaction in progress</exception>
		/// <param name="isolation">The desired transaction isolation level</param>
		/// <param name="cancellationToken">Optional cancelation token</param>
		void StartTransactionAsync(IsolationLevel isolation, CancellationToken cancellationToken = default);

		/// <summary>
		/// Begins a transaction at the ReadCommitted isolation level
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an excpetion if there is already a transaction in progress</exception>
		void StartTransaction();

		/// <summary>
		/// Begins a transaction at the ReadCommitted isolation level
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an excpetion if there is already a transaction in progress</exception>
		/// <param name="cancellationToken">Optional cancelation token</param>
		void StartTransactionAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Commits an active transaction
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an exception if there is not a transaction already in progress</exception>
		void CommitTransaction();

		/// <summary>
		/// Commits an active transaction
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an exception if there is not a transaction already in progress</exception>
		/// <param name="cancellationToken">Optional cancelation token</param>
		void CommitTransactionAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Rolls back the changes for a given transaction
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an exception if there is not a transaction already in progress</exception>
		void RollbackTransaction();

		/// <summary>
		/// Rolls back the changes for a given transaction
		/// </summary>
		/// <exception cref="InvalidOperationException">Will throw an exception if there is not a transaction already in progress</exception>
		/// <param name="cancellationToken">Optional cancelation token</param>
		void RollbackTransactionAsync(CancellationToken cancellationToken = default);

		void EnlistTransaction(IDbTransaction transaction);

		IDbTransaction GetCurrentTransaction();
	}
}
