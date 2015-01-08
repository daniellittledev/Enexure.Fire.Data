using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	internal sealed class CommandWrapper : IDbCommandAsync
	{
		private readonly DbSession session;
		private readonly IDbCommand command;

		internal CommandWrapper(DbSession session, IDbCommand command)
		{
			this.session = session;
			this.command = command;
		}

		public IDbConnection Connection {
			get { return command.Connection; }
			set { command.Connection = value; }
		}
		public IDbTransaction Transaction
		{
			get { return command.Transaction; }
			set { command.Transaction = value; }
		}
		public string CommandText
		{
			get { return command.CommandText; }
			set { command.CommandText = value; }
		}
		public int CommandTimeout
		{
			get { return command.CommandTimeout; }
			set { command.CommandTimeout = value; }
		}
		public CommandType CommandType
		{
			get { return command.CommandType; }
			set { command.CommandType = value; }
		}
		public IDataParameterCollection Parameters
		{
			get { return command.Parameters; }
		}
		public UpdateRowSource UpdatedRowSource
		{
			get { return command.UpdatedRowSource; }
			set { command.UpdatedRowSource = value; }
		}

		public void Prepare()
		{
			session.ReadyDatabase();
			command.Prepare();
		}

		public void Cancel()
		{
			command.Cancel();
		}

		public IDbDataParameter CreateParameter()
		{
			return command.CreateParameter();
		}

		public int ExecuteNonQuery()
		{
			session.ReadyDatabase();
			return command.ExecuteNonQuery();
		}

		public Task<int> ExecuteNonQueryAsync()
		{
			return ExecuteNonQueryAsync(CancellationToken.None);
		}

		public Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			session.ReadyDatabase();
			return ((DbCommand)command).ExecuteNonQueryAsync(cancellationToken);
		}

		public object ExecuteScalar()
		{
			session.ReadyDatabase();
			return command.ExecuteScalar();
		}

		public Task<object> ExecuteScalarAsync()
		{
			return ExecuteScalarAsync(CancellationToken.None);
		}

		public Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
		{
			session.ReadyDatabase();
			return ((DbCommand)command).ExecuteScalarAsync(cancellationToken);
		}

		public IDataReader ExecuteReader()
		{
			session.ReadyDatabase();
			return command.ExecuteReader();
		}

		public Task<IDataReader> ExecuteReaderAsync()
		{
			return ExecuteReaderAsync(CancellationToken.None);
		}

		public async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
		{
			session.ReadyDatabase();
			return await ((DbCommand)command).ExecuteReaderAsync(cancellationToken);
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			if (behavior == CommandBehavior.CloseConnection) {
				throw new NotSupportedException("The CommandBehavior CloseConnection is not supported");
			}

			session.ReadyDatabase();
			return command.ExecuteReader();
		}

		public void Dispose()
		{
			command.Dispose();
		}
	}
}
