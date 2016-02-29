using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly IsolationLevel isolationLevel;
		private DbTransaction transaction;
		private readonly DbConnection connection;

		public DbConnection Connection
		{
			get { return connection; }
		}

		public DbTransaction Transaction
		{
			get { return GetOrCreateTransaction(); }
		}

		public UnitOfWork(DbConnection connection, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
			: this(connection, null, isolationLevel)
		{

		}

		public UnitOfWork(DbConnection connection, DbTransaction transaction, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			if (connection == null) throw new ArgumentNullException("connection", "You must specify a connection");

			this.connection = connection;
			this.isolationLevel = isolationLevel;
			this.transaction = transaction;
		}

		internal DbTransaction GetOrCreateTransaction()
		{
			if (Connection.State == ConnectionState.Closed) {
				Connection.Open();
			}

			return GetTransaction();
		}

		internal Task<DbTransaction> GetOrCreateTransactionAsync()
		{
			return GetOrCreateTransactionAsync(CancellationToken.None);
		}

		internal async Task<DbTransaction> GetOrCreateTransactionAsync(CancellationToken cancellationToken)
		{
			var spinner = WaitForConnection(cancellationToken);
			var timeout = Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
			if (await Task.WhenAny(spinner, timeout) == timeout)
			{
				throw new TimeoutException("Timeout expired while waiting for the connection to open");
			}

			if (Connection.State == ConnectionState.Closed) {
				await Connection.OpenAsync(cancellationToken);
			}
			return GetTransaction();
		}

		private async Task WaitForConnection(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested 
				&& Connection.State == ConnectionState.Connecting)
			{
				await Task.Delay(1, cancellationToken);
			}
		}

		private DbTransaction GetTransaction()
		{
			return transaction ?? (transaction = Connection.BeginTransaction(isolationLevel));
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
			var command = Connection.CreateCommand();
			return command;
		}

		public bool IsConnectionOpen
		{
			get
			{
				return Connection.State != ConnectionState.Closed &&
					   Connection.State != ConnectionState.Broken;
			}
		}

		public void Dispose()
		{
			EndCurrentTransaction();
			Connection.Close();
			Connection.Dispose();
		}

		public void Commit()
		{
			if (transaction != null) {
				transaction.Commit();
			}
		}

		public void Rollback()
		{
			if (transaction != null) {
				transaction.Rollback();
			}
		}

	}
}
