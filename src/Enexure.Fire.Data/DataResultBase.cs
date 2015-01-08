using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Enexure.Fire.Data
{
	public abstract class DataResultBase
	{
		internal static void GetValue<T>(DbDataReader dataReader, Mapper mapper, int length, Dictionary<int, string> keyMappings, List<T> list)
		{
			var row = mapper.GetRowInstance();
			var rowMapper = mapper.GetMapper(row);

			for (var i = 0; i < length; i++) {
				rowMapper(keyMappings[i], dataReader.GetValue(i));
			}

			list.Add((T)row);
		}

		protected static Dictionary<int, string> GetKeyMappings(DbDataReader dataReader, int length)
		{
			return Enumerable
				.Range(0, length)
				.ToDictionary(index => index, dataReader.GetName);
		}
	}
}