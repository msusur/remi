using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ReMi.Common.Utils.Repository
{
	public interface IRepository<TEntity> : IDisposable
	{
		TEntity GetByPrimaryKey<TKey>(TKey primaryKey);

		IQueryable<TEntity> Entities { get; }

		void Insert(TEntity entity);
        void Insert(IEnumerable<TEntity> entities);

        ChangedFields<TEntity> Update(TEntity entity);
        ChangedFields<TEntity> Update(Expression<Func<TEntity, bool>> entitySearcher, Action<TEntity> entityUpdater);
		
		void Delete(TEntity entity);
	    void Delete(IEnumerable<TEntity> entities);
    }
}
