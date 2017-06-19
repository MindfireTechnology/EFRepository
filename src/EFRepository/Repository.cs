using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

#if DOTNETFULL
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif

namespace EFRepository
{
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
	{
		protected DbContext DataContext;
		protected DbSet<TEntity> InternalSet;
		protected PropertyInfo[] KeyProperties;
		protected bool OwnsDataContext;

		public event Action<TEntity> ItemAdded;
		public event Action<TEntity> ItemModified;
		public event Action<TEntity> ItemDeleted;

		public virtual IQueryable<TEntity> Entity { get => InternalSet; }

		public Repository(DbContext context, bool ownsDataContext = true)
		{
			DataContext = context ?? throw new ArgumentNullException(nameof(context));
#if DOTNETFULL
			DataContext.Configuration.AutoDetectChangesEnabled = false;
			DataContext.Configuration.ProxyCreationEnabled = false;
#else
			DataContext.ChangeTracker.AutoDetectChangesEnabled = false;
			DataContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
#endif
			OwnsDataContext = ownsDataContext;
			InternalSet = DataContext.Set<TEntity>();
			SetupKeyProperty();
		}

		public virtual TEntity Find(params object[] keys)
		{
			return InternalSet.Find(keys);
		}

		public virtual void AddOrUpdate(params TEntity[] values)
		{
			AddOrUpdate(values.AsEnumerable());
		}

		public virtual void AddOrUpdate(IEnumerable<TEntity> collection)
		{
			foreach (var entity in collection ?? throw new ArgumentNullException(nameof(collection)))
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
					var entry = GetEntryByKey(entity);
					if (entry == null)
					{
						InternalSet.Attach(entity);
						entry = DataContext.Entry(entity);
					}
					else if (entry.Entity.GetHashCode() != entity.GetHashCode()) // Objects are NOT the same!
					{
						throw new NotSupportedException("A different entity object with the same key already exists in the ChangeTracker");
					}
					
					entry.State = EntityState.Modified;
					ItemModified?.Invoke(entity);
				}
			}
		}

		public virtual void Delete(params object[] keys)
		{
			TEntity value = CreateKeyEntity(keys);

			InternalSet.Attach(value);
			var entry = GetEntryByKey(value);
			entry.State = EntityState.Deleted;

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

		public virtual int Save()
		{
			// Do we need to call DetechChanges?
			return DataContext.SaveChanges();
		}

		public virtual Task<int> SaveAsync()
		{
			return DataContext.SaveChangesAsync();
		}

		public virtual Task<int> SaveAsync(CancellationToken cancellationToken)
		{
			return DataContext.SaveChangesAsync(cancellationToken);
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
		}

		protected virtual TEntity CreateKeyEntity(object[] keyValues)
		{
			if (KeyProperties.Length != keyValues.Length)
				throw new ArgumentOutOfRangeException(nameof(keyValues), $"Expected {KeyProperties.Length} values, but got {keyValues?.Length ?? 0} instead.");

			TEntity result = new TEntity();
			for (int index = 0; index < KeyProperties.Length; index++)
			{
				KeyProperties[index].SetValue(result, keyValues[index]);
			}

			return result;
		}

#if DOTNETFULL
		public virtual DbEntityEntry<TEntity> GetEntryByKey(TEntity entity)
#else
		public EntityEntry<TEntity> GetEntryByKey(TEntity entity)
#endif
		{
			return DataContext.ChangeTracker.Entries<TEntity>().SingleOrDefault(n => KeysEqual(n.Entity, entity));
		}

		protected virtual bool KeysEqual(TEntity value1, TEntity value2)
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

		protected virtual bool IsNew(TEntity entity)
		{
			foreach (var keyField in KeyProperties)
			{
				object value = keyField.GetValue(entity);

				object defaultValue = Activator.CreateInstance(keyField.PropertyType);

				if (value.Equals(defaultValue))
					return true;
			}

			return false;
		}
	}
}
