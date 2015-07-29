using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Enexure.Fire.Data
{
	public class DataResult : DataResultBase, IDataResult
	{
		private readonly DbDataReader dataReader;
		
		public DataResult(DbDataReader dataReader)
		{
			this.dataReader = dataReader;
		}

		public IList<T> ToList<T>()
		{
			return ToEnumerable<T>().ToList();
		}

		public IEnumerable<T> ToEnumerable<T>()
		{
			var mapper = new Mapper(typeof(T));

			while (dataReader.Read())
			{
				yield return mapper.GetRow<T>(dataReader);
			}
		}

		public T Single<T>()
		{
			return ToList<T>().Single();
		}

		public T SingleOrDefault<T>()
			where T : class
		{
			return ToList<T>().SingleOrDefault();
		}

		public void Dispose()
		{
			dataReader.Dispose();
		}
	}
}