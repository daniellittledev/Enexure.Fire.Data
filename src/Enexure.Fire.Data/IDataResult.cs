using System;
using System.Collections.Generic;

namespace Enexure.Fire.Data
{
	public interface IDataResult : IDisposable
	{
		IList<T> ToList<T>();

		IEnumerable<T> ToEnumerable<T>();

		T Single<T>();

		T SingleOrDefault<T>() where T : class;
	}
}