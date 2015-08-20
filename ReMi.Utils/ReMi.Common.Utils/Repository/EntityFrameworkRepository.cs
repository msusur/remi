using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Instrumentation;

namespace ReMi.Common.Utils.Repository
{
    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;
        private bool _disposed = false;

        public EntityFrameworkRepository(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException("unitOfWork");
            }

            _unitOfWork = unitOfWork;
            _dbContext = unitOfWork.DbContext;
            _dbSet = unitOfWork.DbContext.Set<TEntity>();
        }

        /// <summary>
        /// Returns the object with the primary key specifies
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="primaryKey">The primary key</param>
        /// <returns>The founded entity</returns>
        public virtual TEntity GetByPrimaryKey<TKey>(TKey primaryKey)
        {
            return _dbSet.Find(primaryKey);
        }

        /// <summary>
        /// Returns all the entities in the repository. This can be used in linq statements and resulting query will be run against the db.
        /// </summary>
        public virtual IQueryable<TEntity> Entities
        {
            get
            {
                return _dbSet.AsQueryable<TEntity>();
            }
        }

        public virtual void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
            _dbContext.SaveChanges();
        }

        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Add(entity);
            }
            _dbContext.SaveChanges();
        }

        public virtual ChangedFields<TEntity> Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;

            var changeSet = GetChanges(_dbContext);

            _dbContext.SaveChanges();

            return changeSet;
        }

        public virtual ChangedFields<TEntity> Update(Expression<Func<TEntity, bool>> entitySearcher, Action<TEntity> entityUpdater)
        {
            var entity = _dbSet.SingleOrDefault(entitySearcher);
            if (entity == null)
                throw new InstanceNotFoundException();

            entityUpdater(entity);

            var changeSet = GetChanges(_dbContext);

            _dbContext.SaveChanges();

            return changeSet;
        }

        public virtual void Delete(TEntity entity)
        {
            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
            _dbContext.SaveChanges();
        }

        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            var localItems = entities.ToList();
            foreach (var entity in localItems)
            {
                if (_dbContext.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
            }

            _dbSet.RemoveRange(localItems);

            _dbContext.SaveChanges();
        }

        #region IDispose implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _unitOfWork.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private ChangedFields<TEntity> GetChanges(DbContext context)
        {
            var result = new ChangedFields<TEntity>();

            var entity = context.ChangeTracker.Entries<TEntity>().FirstOrDefault();
            if (entity != null && entity.OriginalValues != null)
            {
                var properties = entity.OriginalValues.PropertyNames;
                if (properties != null)
                    foreach (var propertyName in properties)
                    {
                        var property = entity.Property(propertyName);

                        if ((property.CurrentValue == null && property.OriginalValue != null) ||
                            (property.CurrentValue != null && !property.CurrentValue.Equals(property.OriginalValue)))
                            result.Add(new ChangedField(propertyName, property.OriginalValue, property.CurrentValue));
                    }
            }

            return result;
        }

    }
}
