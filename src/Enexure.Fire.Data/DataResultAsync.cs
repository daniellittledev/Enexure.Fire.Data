using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public class DataResultAsync : DataResultBase, IDataResultAsync
	{
		private readonly DbDataReader dataReader;

		public DataResultAsync(DbDataReader dataReader)
		{
			this.dataReader = dataReader;
		}

		public async Task<IList<T>> ToListAsync<T>()
		{
			var reader = dataReader;

			using (reader) {
				var list = new List<T>();
				var mapper = new Mapper(typeof(T));
				var length = reader.FieldCount;
				var keyMappings = GetKeyMappings(reader, length);

				while (await reader.ReadAsync()) {
					GetValue(reader, mapper, length, keyMappings, list);
				}
				return list;
			}
		}

		public async Task<T> SingleAsync<T>()
		{
			return (await ToListAsync<T>()).Single();
		}

		public async Task<T> SingleOrDefaultAsync<T>()
			where T : class 
		{
			return (await ToListAsync<T>()).SingleOrDefault();
		}

	    public void Dispose()
	    {
            dataReader.Dispose();
        }
	}
}