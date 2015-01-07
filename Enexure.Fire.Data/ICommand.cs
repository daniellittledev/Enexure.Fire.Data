using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public interface ICommand
	{
		int ExecuteNonQuery();
		Task<int> ExecuteNonQueryAsync();
		Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);

		T ExecuteScalar<T>();
		Task<T> ExecuteScalarAsync<T>();
		Task<T> ExecuteScalarAsync<T>(CancellationToken cancellationToken);

		IDataResult ExecuteQuery();
		IDataResultAsync ExecuteQueryAsync();
		IDataResultAsync ExecuteQueryAsync(CancellationToken cancellationToken);
	}
}
