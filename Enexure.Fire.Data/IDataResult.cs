using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public interface IDataResult
	{
		IList<T> ToList<T>();

		T Single<T>();

		T SingleOrDefault<T>() where T : class;
	}

	public interface IDataResultAsync
	{
		Task<IList<T>> ToListAsync<T>();

		Task<T> SingleAsync<T>();

		Task<T> SingleOrDefaultAsync<T>() where T : class;
	}
}