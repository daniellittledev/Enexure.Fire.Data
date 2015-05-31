using System;

namespace Enexure.Fire.Data
{
	public interface IUnitOfWork : IDisposable
	{
		void Commit();
		void Rollback();
	}
}