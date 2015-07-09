using System;
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

				var key = keyMappings[i];
				try {
					rowMapper(key, dataReader.GetValue(i));
				} catch (Exception ex) {
					throw new CouldNotSetPropertyException(key, ex);
				}
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

	internal class CouldNotSetPropertyException : Exception
	{
		public CouldNotSetPropertyException(string key, Exception exception)
			: base(string.Format("Could not set property {0}", key), exception)
		{
			
		}
	}
}