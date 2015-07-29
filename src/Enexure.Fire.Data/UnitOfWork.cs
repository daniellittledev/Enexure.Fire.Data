using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly DbConnection connection;
		private readonly IsolationLevel isolationLevel;

		private DbTransaction transaction;

		public UnitOfWork(DbConnection connection, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			if (connection == null) throw new ArgumentNullException("connection", "You must specify a connection");

			this.connection = connection;
			this.isolationLevel = isolationLevel;
		}

		internal DbTransaction GetOrCreateTransaction()
		{
			if (connection.State == ConnectionState.Closed) {
				connection.Open();
			}

			return GetTransaction();
		}

		internal Task<DbTransaction> GetOrCreateTransactionAsync()
		{
			return GetOrCreateTransactionAsync(CancellationToken.None);
		}

		internal async Task<DbTransaction> GetOrCreateTransactionAsync(CancellationToken cancellationToken)
		{
			if (connection.State == ConnectionState.Closed) {
				await connection.OpenAsync(cancellationToken);
			}
			return GetTransaction();
		}

		private DbTransaction GetTransaction()
		{
			return transaction ?? (transaction = connection.BeginTransaction(isolationLevel));
		}

		private void EndCurrentTransaction()
		{
			if (transaction != null) {
				transaction.Dispose();
			}

			transaction = null;
		}

		internal DbCommand CreateCommand()
		{
			var command = connection.CreateCommand();
			return command;
		}

		public bool IsConnectionOpen
		{
			get
			{
				return connection.State != ConnectionState.Closed &&
					   connection.State != ConnectionState.Broken;
			}
		}

		public void Dispose()
		{
			EndCurrentTransaction();
			connection.Close();
			connection.Dispose();
		}

		public void Commit()
		{
			if (transaction != null) {

				transaction.Commit();
				EndCurrentTransaction();
			}
		}

		public void Rollback()
		{
			if (transaction != null) {

				transaction.Rollback();
				EndCurrentTransaction();
			}
		}

	}
}
