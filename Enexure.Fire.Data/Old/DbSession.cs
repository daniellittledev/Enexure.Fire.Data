using System;
using System.Data;

namespace Enexure.Fire.Data
{
	public enum TransactionMode
	{
		Auto,
		Off
	}

	public class DbSession : IDbSession
	{
		private readonly IDbConnection connection;
		private readonly TransactionMode transactionMode;
		private readonly IsolationLevel isolationLevel;

		private IDbTransaction transaction;
		private bool isOpen;
		private bool hasExecuted;
		private bool isDisposed;

		public DbSession(IDbConnection connection)
		{
			this.connection = connection;
			this.transactionMode = TransactionMode.Auto;
			this.isolationLevel = IsolationLevel.Unspecified;
		}

		public DbSession(IDbConnection connection, TransactionMode transactionMode)
		{
			this.connection = connection;
			this.transactionMode = transactionMode;
			this.isolationLevel = IsolationLevel.Unspecified;
		}

		public DbSession(IDbConnection connection, IsolationLevel isolationLevel)
		{
			this.connection = connection;
			this.transactionMode = TransactionMode.Auto;
			this.isolationLevel = isolationLevel;
		}

		internal void ReadyDatabase()
		{
			hasExecuted = true;
		}

		public void Open()
		{
			if (isDisposed) throw new ObjectDisposedException("Session is disposed");

			if (!isOpen) {
				connection.Open();
				isOpen = true;
			}
		}

		public void Close()
		{
			if (isDisposed) throw new ObjectDisposedException("Session is disposed");

			if (isOpen) {
				connection.Close();
				isOpen = false;
			}
		}

		public IDbCommand CreateCommand()
		{
			return CreateAsyncCommand();
		}


		public IDbCommandAsync CreateAsyncCommand()
		{
			Open();

			var command = connection.CreateCommand();

			if (transactionMode == TransactionMode.Auto && transaction == null) {
				command.Transaction = transaction = connection.BeginTransaction(isolationLevel);
			}

			return new CommandWrapper(this, command);
		}

		public void Commit()
		{
			if (isDisposed) throw new ObjectDisposedException("Session is disposed");

			if (hasExecuted || transaction != null) {
				transaction.Commit();
				transaction.Dispose();
				transaction = null;
			}

			hasExecuted = true;
		}

		public void Rollback()
		{
			if (isDisposed) throw new ObjectDisposedException("Session is disposed");

			if (hasExecuted || transaction != null) {
				transaction.Rollback();
				transaction.Dispose();
				transaction = null;
			}

			hasExecuted = true;
		}

		public void DisposeTransaction()
		{
			if (isDisposed) throw new ObjectDisposedException("Session is disposed");

			if (hasExecuted || transaction != null) {
				transaction.Dispose();
				transaction = null;
			}

			hasExecuted = true;
		}

		public IDbTransaction BeginTransaction()
		{
			return new TransactionWrapper(this);
		}

		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			throw new NotSupportedException("DbSession controlls the transaction level, use the constructor instead");
		}

		public void ChangeDatabase(string databaseName)
		{
			connection.ChangeDatabase(databaseName);
		}

		public string ConnectionString {
			get { return connection.ConnectionString; }
			set { connection.ConnectionString = value; }
		}

		public IsolationLevel IsolationLevel
		{
			get { return isolationLevel; }
		}

		public int ConnectionTimeout
		{
			get { return connection.ConnectionTimeout; }
		}

		public string Database
		{
			get { return connection.Database; }
		}

		public ConnectionState State
		{
			get { return connection.State; }
		}

		public void Dispose()
		{
			DisposeTransaction();
			connection.Dispose();
			isDisposed = true;
		}

	}
}
