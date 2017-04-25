using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFRepository
{
	public interface IObjectRepository<TObject, TEntity> : IDisposable where TEntity : class, new() where TObject : class, new()
	{
		/// <summary>Queriable Entity</summary>
		IQueryable<TEntity> Entity { get; }

		/// <summary>
		/// Find based on key
		/// </summary>
		/// <param name="keys">The key(s) for the table</param>
		/// <returns>Entity if found, otherwise null</returns>
		TObject Find(params object[] keys);

		/// <summary>
		/// Add or update an entity
		/// </summary>
		/// <param name="values">Entities to add</param>
		void AddOrUpdate(params TObject[] values);

		/// <summary>
		/// Add or update an entity
		/// </summary>
		/// <param name="collection">Entities to add</param>
		void AddOrUpdate(IEnumerable<TObject> collection);

		/// <summary>
		/// Delete entity by key
		/// </summary>
		/// <param name="keys">The key(s) for the table</param>
		void Delete(params object[] keys);

		/// <summary>
		/// Delete one or more entities
		/// </summary>
		/// <param name="values">Entities to delete</param>
		void Delete(params TObject[] values);

		/// <summary>
		/// Delete one or more entities
		/// </summary>
		/// <param name="collection">Entities to delete</param>
		void Delete(IEnumerable<TObject> collection);

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

		/// <summary>Remove any staged changes from the Repository</summary>
		void AbandonChanges();

		/// <summary>Event that fires when an item is added</summary>
		event Action<TObject> ItemAdded;

		/// <summary>Event that fires when an itemis modified</summary>
		event Action<TObject> ItemModified;

		/// <summary>Event that fires when an item is deleted</summary>
		event Action<TObject> ItemDeleted;
	}
}
