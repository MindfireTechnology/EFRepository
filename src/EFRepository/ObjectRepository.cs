using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFRepository
{
	public class ObjectRepository<TObject, TEntity> : IObjectRepository<TObject, TEntity> where TObject : class, new() where TEntity : class, new()
	{
		protected DbContext Context;
		protected Func<TObject, TEntity> ObjectMapper;
		protected Func<TEntity, TObject> EntityMapper;

		public event Action<TObject> ItemAdded;
		public event Action<TObject> ItemModified;
		public event Action<TObject> ItemDeleted;
		public IQueryable<TEntity> Entity => throw new NotImplementedException();

		public ObjectRepository(DbContext context, Func<TObject, TEntity> objectMapper, Func<TEntity, TObject> entityMapper)
		{
			/*
			 * var results = repo.GetResults(repo.Entity.ByDateRance(null, null)...);
			 * 
			 */
		}

		IEnumerable<TObject> GetResults(IQueryable<TEntity> query)
		{
			return query.AsEnumerable().Select(n => EntityMapper(n));
		}


		public void AbandonChanges()
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdate(params TObject[] values)
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdate(IEnumerable<TObject> collection)
		{
			throw new NotImplementedException();
		}

		public void Delete(params object[] keys)
		{
			throw new NotImplementedException();
		}

		public void Delete(params TObject[] values)
		{
			throw new NotImplementedException();
		}

		public void Delete(IEnumerable<TObject> collection)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public TObject Find(params object[] keys)
		{
			throw new NotImplementedException();
		}

		public int Save()
		{
			throw new NotImplementedException();
		}

		public Task<int> SaveAsync()
		{
			throw new NotImplementedException();
		}

		public Task<int> SaveAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
