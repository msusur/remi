using System;
using System.Data.Entity;
using System.Transactions;

namespace ReMi.Common.Utils.Repository
{
	public class EntityFrameworkUnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext, new()
	{
		private TransactionScope _transaction;
		private bool _disposed;
        private readonly TDbContext _dbContext;

		public EntityFrameworkUnitOfWork()
        {
            _dbContext = new TDbContext();
			_disposed = false;
        }

		public DbContext DbContext
		{
			get { return _dbContext; }
		}

		public void StartTransaction()
        {
           _transaction = new TransactionScope();
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
            _transaction.Complete();
        }

		#region IDisposed

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_dbContext.Dispose();
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

	}
}
