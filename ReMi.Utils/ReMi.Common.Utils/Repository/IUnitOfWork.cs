using System;
using System.Data.Entity;

namespace ReMi.Common.Utils.Repository
{
	public interface IUnitOfWork : IDisposable
	{
		/// <summary>
		/// Call this to commit the unit of work
		/// </summary>
		void Commit();

		/// <summary>
		/// Return the database reference for this unit of work
		/// </summary>
		DbContext DbContext { get; }

		/// <summary>
		/// Starts a transaction on this unit of work
		/// </summary>
		void StartTransaction();

	}
}
