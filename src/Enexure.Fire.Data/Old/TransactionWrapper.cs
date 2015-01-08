using System;
using System.Data;

namespace Enexure.Fire.Data
{
	public class TransactionWrapper : IDbTransaction
	{
		private readonly IDbSession dbSession;

		public TransactionWrapper(IDbSession dbSession)
		{
			this.dbSession = dbSession;
		}

		public void Dispose()
		{
			dbSession.DisposeTransaction();
		}

		public void Commit()
		{
			dbSession.Commit();
		}

		public void Rollback()
		{
			dbSession.Rollback();
		}

		public IDbConnection Connection {
			get { return dbSession; }
		}
		public IsolationLevel IsolationLevel {
			get { return dbSession.IsolationLevel; }
		}
	}
}
