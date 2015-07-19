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
			var list = new List<T>();
			var mapper = new Mapper(typeof(T));
			var length = dataReader.FieldCount;
			var keyMappings = GetKeyMappings(dataReader, length);

			while (dataReader.Read()) {
				GetValue(dataReader, mapper, length, keyMappings, list);
			}
			return list;
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