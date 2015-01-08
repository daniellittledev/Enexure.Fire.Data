using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public class UnitOfWork : IDisposable
	{
		private readonly DbConnection connection;
		private readonly IsolationLevel isolationLevel;

		private DbTransaction transaction;

		public UnitOfWork(DbConnection connection, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			this.connection = connection;
			this.isolationLevel = isolationLevel;
		}

		internal void Begin()
		{
			if (connection.State == ConnectionState.Closed) {
				connection.Open();
			}
		}

		internal Task BeginAsync()
		{
			return BeginAsync(CancellationToken.None);
		}

		internal Task BeginAsync(CancellationToken cancellationToken)
		{
			if (connection.State == ConnectionState.Closed) {
				return connection.OpenAsync(cancellationToken);
			}
			return Task.FromResult(true);
		}

		private DbTransaction GetCurrentTransaction()
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

		public DbCommand CreateCommand()
		{
			var command = connection.CreateCommand();
			command.Transaction = GetCurrentTransaction();
			return command;
		}

		public void Dispose()
		{
			EndCurrentTransaction();
			connection.Close();
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
