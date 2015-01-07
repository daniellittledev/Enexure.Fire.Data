using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public interface IDbCommandAsync : IDbCommand
	{
		Task<int> ExecuteNonQueryAsync();
		Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);

		Task<object> ExecuteScalarAsync();
		Task<object> ExecuteScalarAsync(CancellationToken cancellationToken);

		Task<IDataReader> ExecuteReaderAsync();
		Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken);
	}
}
