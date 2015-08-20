using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ReMi.Common.Utils.Repository
{
	public static class RepositoryExtensions
	{
		/// <summary>
		/// returns a single entity satisfied by the search criteria or null if nothing is found
		/// </summary>
		/// <typeparam name="TEntity">type of the entity</typeparam>
		/// <param name="searchCriteria">the criteria search filter</param>
		/// <returns>Entities matching the criteria or null if no entity matches the search criteria</returns>
		/// <thows>ArgumentNullException if criteria is not defined</thows>
		public static TEntity GetSatisfiedBy<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> searchCriteria) where TEntity : class
		{
			if (searchCriteria == null)
			{
				throw new ArgumentNullException("searchCriteria");
			}

			return repository.Entities.SingleOrDefault(searchCriteria);
		}

		/// <summary>
		/// returns a collection of entities satisfied by the search criteria
		/// </summary>
		/// <param name="searchCriteria">the criteria search filter</param>
		/// <typeparam name="TEntity">type of the entity</typeparam>
		/// <returns>collection of entities matching the criteria</returns>
		public static IEnumerable<TEntity> GetAllSatisfiedBy<TEntity>(
			this IRepository<TEntity> repository, 
			Expression<Func<TEntity, bool>> searchCriteria = null) 
				where TEntity : class
		{
			if (searchCriteria == null)
			{
				return repository.Entities.ToList();
			}

			return repository.Entities.Where(searchCriteria).ToList();
		}

		/// <summary>
		/// returns a collection of entities satisfied by the search criteria
		/// </summary>
		/// <param name="searchCriteria">the criteria search filter</param>
		/// <param name="orderCriteria">the criteria to sort the results</param>
		/// <typeparam name="TEntity">type of the entity</typeparam>
		/// <returns>collection of entities matching the criteria</returns>
		public static IEnumerable<TEntity> GetAllSortedSatisfiedBy<TEntity>(
			this IRepository<TEntity> repository, 
			Expression<Func<TEntity, bool>> searchCriteria,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderCriteria = null) 
				where TEntity : class
		{
			if (orderCriteria == null)
			{
				return repository.GetAllSatisfiedBy(searchCriteria);
			}

			IQueryable<TEntity> query = repository.Entities;
			if (searchCriteria != null)
			{
				query = query.Where(searchCriteria);
			}

			return orderCriteria(query).ToList();
		}


		/// <summary>
		/// deletes entity with given primary key
		/// </summary>
		/// <typeparam name="TKey">type of the primary key (typically int)</typeparam>
		/// <typeparam name="TEntity">type of the entity</typeparam>
		/// <param name="primaryKey">primary key value of the entity to delete</param>
		public static void Delete<TEntity, TKey>(this IRepository<TEntity> repository, TKey primaryKey) where TEntity : class
		{
			TEntity entityToDelete = repository.GetByPrimaryKey(primaryKey);
			repository.Delete(entityToDelete);
		}

	}
}
