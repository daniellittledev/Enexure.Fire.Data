using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public interface ICommand : IDisposable
	{
		int ExecuteNonQuery();
		Task<int> ExecuteNonQueryAsync();
		Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);

		T ExecuteScalar<T>();
		Task<T> ExecuteScalarAsync<T>();
		Task<T> ExecuteScalarAsync<T>(CancellationToken cancellationToken);

		IDataResult ExecuteQuery();
		Task<IDataResultAsync> ExecuteQueryAsync();
		Task<IDataResultAsync> ExecuteQueryAsync(CancellationToken cancellationToken);
	}
}
