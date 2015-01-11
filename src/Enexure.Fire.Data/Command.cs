using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public class Command : ICommand
	{
		private readonly DbCommand command;
		private readonly UnitOfWork unitOfWork;

		internal Command(DbCommand command, UnitOfWork unitOfWork)
		{
			this.command = command;
			this.unitOfWork = unitOfWork;
		}

		private void ApplyTransaction()
		{
			command.Transaction = unitOfWork.GetOrCreateTransaction();
		}

		private async Task ApplyTransactionAsync()
		{
			command.Transaction = await unitOfWork.GetOrCreateTransactionAsync();
		}

		private async Task ApplyTransactionAsync(CancellationToken cancellationToken)
		{
			command.Transaction = await unitOfWork.GetOrCreateTransactionAsync(cancellationToken);
		}

		public int ExecuteNonQuery()
		{
			ApplyTransaction();
			return command.ExecuteNonQuery();
		}

		public async Task<int> ExecuteNonQueryAsync()
		{
			await ApplyTransactionAsync();
			return await command.ExecuteNonQueryAsync();
		}

		public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			await ApplyTransactionAsync(cancellationToken);
			return await command.ExecuteNonQueryAsync(cancellationToken);
		}

		public T ExecuteScalar<T>()
		{
			ApplyTransaction();
			return (T)command.ExecuteScalar();
		}

		public async Task<T> ExecuteScalarAsync<T>()
		{
			await ApplyTransactionAsync();
			return (T) await command.ExecuteScalarAsync();
		}

		public async Task<T> ExecuteScalarAsync<T>(CancellationToken cancellationToken)
		{
			await ApplyTransactionAsync(cancellationToken);
			return (T) await command.ExecuteScalarAsync(cancellationToken);
		}

		public IDataResult ExecuteQuery()
		{
			ApplyTransaction();
			return new DataResult(command.ExecuteReader());
		}

		public IDataResultAsync ExecuteAsyncQuery()
		{
			return new DataResultAsync(async () => {
				await ApplyTransactionAsync();
				return await command.ExecuteReaderAsync();
			});
		}

		public IDataResultAsync ExecuteAsyncQuery(CancellationToken cancellationToken)
		{
			return new DataResultAsync(async () => {
				await ApplyTransactionAsync(cancellationToken);
				return await command.ExecuteReaderAsync(cancellationToken);
			});
		}
	}
}