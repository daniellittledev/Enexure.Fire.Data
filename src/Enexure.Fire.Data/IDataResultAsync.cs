using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public interface IDataResultAsync : IDisposable
	{
		Task<IList<T>> ToListAsync<T>();

		Task ToCallbacks<T>(Action<T> callback);


		Task<T> SingleAsync<T>();

		Task<T> SingleOrDefaultAsync<T>() where T : class;
	}
}