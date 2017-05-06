using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

#if NETSTANDARD1_4
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace EFRepository
{
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
	{
		protected DbContext DataContext;
		protected DbSet<TEntity> InternalSet;
		protected PropertyInfo[] KeyProperties;
		protected MethodInfo DefaultMethod;
		protected bool OwnsDataContext;

		public event Action<TEntity> ItemAdded;
		public event Action<TEntity> ItemModified;
		public event Action<TEntity> ItemDeleted;

		public virtual IQueryable<TEntity> Entity { get; protected set; }

		public Repository(DbContext context, bool ownsDataContext = true)
		{
			DataContext = context ?? throw new ArgumentNullException(nameof(context));
			OwnsDataContext = ownsDataContext;
			SetupEntityProperty();
			SetupKeyProperty();
		}

		public virtual TEntity Find(params object[] keys)
		{
			return InternalSet.Find(keys);
		}

		public virtual void AddOrUpdate(params TEntity[] values)
		{
			AddOrUpdate(values);
		}

		public virtual void AddOrUpdate(IEnumerable<TEntity> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			foreach (var entity in collection)
			{
				// Check to see if this is a new entity (by checking the key)
				if (IsNew(entity))
				{
					InternalSet.Add(entity);
					ItemAdded?.Invoke(entity);
				}
				else
				{
					// Is this entity already attached?
					var entry = DataContext.ChangeTracker.Entries<TEntity>().SingleOrDefault(n => KeysEqual(n.Entity, entity));
					if (entry == null)
					{
						// BUG: Entity exists but is not the one they saved here...
						InternalSet.Add(entity);
						entry = DataContext.ChangeTracker.Entries<TEntity>().SingleOrDefault(n => KeysEqual(n.Entity, entity));
					}
					
					entry.State = EntityState.Modified;

					ItemModified?.Invoke(entity);
				}
			}
		}

		public virtual void Delete(params object[] keys)
		{
			TEntity value = Find(keys);
			InternalSet.Remove(value);
			ItemDeleted?.Invoke(value);
		}

		public virtual void Delete(params TEntity[] values)
		{
			Delete(values);
			if (ItemDeleted != null)
				values.ToList().ForEach(n => ItemDeleted?.Invoke(n));
		}

		public virtual void Delete(IEnumerable<TEntity> collection)
		{
			foreach (var entity in collection)
			{
				InternalSet.Remove(entity);
				ItemDeleted?.Invoke(entity);
			}
		}

#if DOTNETFULL
		public virtual int Execute(string sql, IEnumerable<object> parameters)
		{
			return DataContext.Database.ExecuteSqlCommand(sql, parameters);
		}

		public virtual IEnumerable<TEntity> Query(string sql, IEnumerable<object> parameters)
		{
			return DataContext.Database.SqlQuery<TEntity>(sql, parameters.ToArray());
		}
#endif

		public virtual int Save()
		{
			return DataContext.SaveChanges();
		}

		public Task<int> SaveAsync()
		{
			return DataContext.SaveChangesAsync();
		}

		public Task<int> SaveAsync(CancellationToken cancellationToken)
		{
			return DataContext.SaveChangesAsync(cancellationToken);
		}

		public void AbandonChanges()
		{
			// TODO: Actually remove the records from the context.
			DataContext.ChangeTracker.Entries().ToList().ForEach(n => n.State = EntityState.Unchanged);
		}

		public virtual void Dispose()
		{
			if (OwnsDataContext)
				DataContext.Dispose();
		}

		protected virtual void SetupKeyProperty()
		{
			// Get properties of the Entity and look for the key(s)
			var keys = new List<PropertyInfo>();
			foreach (var prop in typeof(TEntity).GetRuntimeProperties())
			{
				if (prop.GetCustomAttribute<KeyAttribute>(true) != null ||
					prop.Name.Equals("ID", StringComparison.CurrentCultureIgnoreCase) ||
					prop.Name.Equals($"{typeof(TEntity).Name}ID", StringComparison.CurrentCultureIgnoreCase))
				{
					keys.Add(prop);
				}
			}

			KeyProperties = keys.ToArray();
			DefaultMethod = GetType().GetRuntimeMethod("Default", null);
		}

		protected virtual void SetupEntityProperty()
		{
			// Look for the type 
			foreach (var prop in DataContext.GetType().GetRuntimeProperties())
			{
#if NETSTANDARD1_4
				if (prop.PropertyType == typeof(DbSet<TEntity>))
#else
				if (prop.PropertyType == typeof(DbSet<TEntity>) || prop.PropertyType == typeof(IDbSet<TEntity>))
#endif
				{
					Entity = (DbSet<TEntity>)prop.GetValue(DataContext);
					InternalSet = (DbSet<TEntity>)prop.GetValue(DataContext);
					return;
				}
			}

			throw new InvalidOperationException("Unable to find type DbSet<TEntity> of IDbSet<TEntity> in data context provided.");
		}

		protected bool KeysEqual(TEntity value1, TEntity value2)
		{
			foreach (var keyfield in KeyProperties)
			{
				object cmp1 = keyfield.GetValue(value1);
				object cmp2 = keyfield.GetValue(value2);

				if (!cmp1.Equals(cmp2))
					return false;
			}

			return true;
		}

		protected bool IsNew(TEntity entity)
		{
			foreach (var keyField in KeyProperties)
			{
				object value = keyField.GetValue(entity);

				object defaultValue = DefaultMethod
					.MakeGenericMethod(keyField.PropertyType)
					.Invoke(null, new object[0]);

				if (value.Equals(defaultValue))
					return true;
			}

			return false;
		}

		/// <summary>Used to get the default value for different key types. Do not remove -- invoked via reflection.</summary>
		private static T Default<T>()
		{
			return default(T);
		}
	}
}
