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

		public int ExecuteNonQuery()
		{
			unitOfWork.Begin();
			return command.ExecuteNonQuery();
		}

		public async Task<int> ExecuteNonQueryAsync()
		{
			await unitOfWork.BeginAsync();
			return await command.ExecuteNonQueryAsync();
		}

		public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			await unitOfWork.BeginAsync(cancellationToken);
			return await command.ExecuteNonQueryAsync(cancellationToken);
		}

		public T ExecuteScalar<T>()
		{
			unitOfWork.Begin();
			return (T)command.ExecuteScalar();
		}

		public async Task<T> ExecuteScalarAsync<T>()
		{
			await unitOfWork.BeginAsync();
			return (T) await command.ExecuteScalarAsync();
		}

		public async Task<T> ExecuteScalarAsync<T>(CancellationToken cancellationToken)
		{
			await unitOfWork.BeginAsync(cancellationToken);
			return (T) await command.ExecuteScalarAsync(cancellationToken);
		}

		public IDataResult ExecuteQuery()
		{
			unitOfWork.Begin();
			return new DataResult(command.ExecuteReader());
		}

		public IDataResultAsync ExecuteQueryAsync()
		{
			return new DataResultAsync(async () => {
				await unitOfWork.BeginAsync();
				return await command.ExecuteReaderAsync();
			});
		}

		public IDataResultAsync ExecuteQueryAsync(CancellationToken cancellationToken)
		{
			return new DataResultAsync(async () => {
				await unitOfWork.BeginAsync(cancellationToken);
				return await command.ExecuteReaderAsync(cancellationToken);
			});
		}
	}
}