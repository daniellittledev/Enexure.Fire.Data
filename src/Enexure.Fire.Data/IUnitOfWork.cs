using System;

namespace Enexure.Fire.Data
{
	public interface IUnitOfWork : IDisposable
	{
		bool IsConnectionOpen { get; }
		void Commit();
		void Rollback();
	}
}