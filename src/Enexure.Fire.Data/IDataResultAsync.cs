using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public interface IDataResultAsync
	{
		Task<IList<T>> ToListAsync<T>();

		Task<T> SingleAsync<T>();

		Task<T> SingleOrDefaultAsync<T>() where T : class;
	}
}