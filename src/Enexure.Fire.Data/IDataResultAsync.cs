using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public interface IDataResultAsync : IDisposable
	{
		Task<IList<T>> ToListAsync<T>();

		Task<T> SingleAsync<T>();

		Task<T> SingleOrDefaultAsync<T>() where T : class;
	}
}