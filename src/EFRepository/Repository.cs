using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

#if NET45_OR_GREATER
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace EFRepository
{
	public class Repository : IRepository
	{
		protected DbContext DataContext;
		protected bool OwnsDataContext;

		public event Action<object> ItemAdding;
		public event Action<object> ItemModifing;
		public event Action<object> ItemDeleting;

		public Repository(DbContext context, bool ownsDataContext = true)
		{
			DataContext = context ?? throw new ArgumentNullException(nameof(context));

			OwnsDataContext = ownsDataContext;
		}

		public virtual IQueryable<TEntity> Query<TEntity>() where TEntity : class, new() => DataContext.Set<TEntity>();

		public virtual IQueryable<TEntity> Join<TEntity>() where TEntity : class, new()
		{
			throw new NotImplementedException();
		}


		public virtual TEntity FindOne<TEntity>(params object[] keys) where TEntity : class, new()
		{
			return DataContext.Set<TEntity>().Find(keys);
		}

		public virtual Task<TEntity> FindOneAsync<TEntity>(params object[] keys) where TEntity : class, new()
		{
			return Task.Run(() => DataContext.Set<TEntity>().Find(keys));
		}

		public virtual void AddNew<TEntity>(params TEntity[] values) where TEntity : class, new()
		{
			foreach (var entity in values ?? throw new ArgumentNullException(nameof(values)))
			{
				DataContext.Set<TEntity>().Add(entity);
				ItemAdding?.Invoke(entity);
			}
		}

		public virtual void AddOrUpdate<TEntity>(params TEntity[] values) where TEntity : class, new()
		{
			AddOrUpdate(values.AsEnumerable());
		}

		public virtual void AddOrUpdate<TEntity>(IEnumerable<TEntity> collection) where TEntity : class, new()
		{
			foreach (var entity in collection ?? throw new ArgumentNullException(nameof(collection)))
			{
				// Check to see if this is a new entity (by checking the key)
				if (IsNew(entity))
				{
					DataContext.Set<TEntity>().Add(entity);
					ItemAdding?.Invoke(entity);
				}
				else
				{
					// Is this entity already attached?
					var entry = GetEntryByKey(entity);
					if (entry.Entity.GetHashCode() != entity.GetHashCode()) // Objects are NOT the same!
					{
						throw new NotSupportedException("A different entity object with the same key already exists in the ChangeTracker");
					}

					entry.State = EntityState.Modified;
					ItemModifing?.Invoke(entity);
				}
			}
		}

		public virtual void DeleteOne<TEntity>(params object[] keys) where TEntity : class, new()
		{
			var value = CreateKeyEntity<TEntity>(keys);

			var entry = GetEntryByKey(value);
			entry.State = EntityState.Deleted;

			ItemDeleting?.Invoke(value);
		}

		public virtual void Delete<TEntity>(params TEntity[] values) where TEntity : class, new()
		{
			Delete(values.AsEnumerable());
		}

		public virtual void Delete<TEntity>(IEnumerable<TEntity> collection) where TEntity : class, new()
		{
			foreach (var entity in collection)
			{
				DataContext.Set<TEntity>().Remove(entity);
				ItemDeleting?.Invoke(entity);
			}
		}

		public virtual int Save()
		{
			CheckDetectChanges();

			return DataContext.SaveChanges();
		}

		public virtual Task<int> SaveAsync()
		{
			CheckDetectChanges();

			return DataContext.SaveChangesAsync();
		}

		public virtual Task<int> SaveAsync(CancellationToken cancellationToken)
		{
			CheckDetectChanges();

			return DataContext.SaveChangesAsync(cancellationToken);
		}

		protected virtual void CheckDetectChanges()
		{
#if NET45_OR_GREATER
			if(!DataContext.Configuration.AutoDetectChangesEnabled && DataContext.Configuration.ProxyCreationEnabled)
#else
			if (!DataContext.ChangeTracker.AutoDetectChangesEnabled && DataContext.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.NoTracking)
#endif
			{
				DataContext.ChangeTracker.DetectChanges();
			}

		}

		public virtual void Dispose()
		{
			if (OwnsDataContext)
				DataContext.Dispose();
		}

		protected PropertyInfo[] GetKeyProperties<TEntity>()
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

			return keys.ToArray();
		}

		protected virtual TEntity CreateKeyEntity<TEntity>(object[] keyValues) where TEntity : class, new()
		{
			var keyProperties = GetKeyProperties<TEntity>();
			if (keyProperties.Length != keyValues.Length)
				throw new ArgumentOutOfRangeException(nameof(keyValues), $"Expected {keyProperties.Length} values, but got {keyValues?.Length ?? 0} instead.");

			var result = new TEntity();
			for (int index = 0; index < keyProperties.Length; index++)
			{
				keyProperties[index].SetValue(result, keyValues[index]);
			}

			return result;
		}

#if NET45_OR_GREATER
		public virtual DbEntityEntry<TEntity> GetEntryByKey<TEntity>(TEntity entity)  where TEntity : class, new()
#else
		public EntityEntry<TEntity> GetEntryByKey<TEntity>(TEntity entity) where TEntity : class, new()
#endif
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			var result = DataContext.ChangeTracker.Entries<TEntity>().SingleOrDefault(n => KeysEqual(n.Entity, entity));

			if(result == null)
			{
				DataContext.Set<TEntity>().Attach(entity);
				result = DataContext.ChangeTracker.Entries<TEntity>().SingleOrDefault(n => KeysEqual(n.Entity, entity));
			}

			return result;
		}

		protected virtual bool KeysEqual<TEntity>(TEntity value1, TEntity value2) where TEntity : class, new()
		{
			foreach (var keyfield in GetKeyProperties<TEntity>())
			{
				object cmp1 = keyfield.GetValue(value1);
				object cmp2 = keyfield.GetValue(value2);

				if (!cmp1.Equals(cmp2))
					return false;
			}

			return true;
		}

		protected virtual bool IsNew<TEntity>(TEntity entity) where TEntity : class, new()
		{
			foreach (var keyField in GetKeyProperties<TEntity>())
			{
				object value = keyField.GetValue(entity);

				// TODO: Check for "AutoGenerated" attribute on this field
				object defaultValue = Activator.CreateInstance(keyField.PropertyType);

				if (value.Equals(defaultValue))
					return true;
			}

			return false;
		}
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
