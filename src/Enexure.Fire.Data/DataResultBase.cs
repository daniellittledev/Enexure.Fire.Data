using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Enexure.Fire.Data
{
	public abstract class DataResultBase
	{
		internal static T GetValue<T>(DbDataReader dataReader, Mapper mapper, Dictionary<int, string> keyMappings)
		{
			var len = dataReader.FieldCount;
			var row = mapper.GetRowInstance(len);
			var rowMapper = mapper.GetMapper(row);

			for (var i = 0; i < len; i++)
			{
				var key = keyMappings[i];
				try
				{
					var value = dataReader.GetValue(i);
					rowMapper(key, value == DBNull.Value ? null : value);
				}
				catch (Exception ex)
				{
					throw new CouldNotSetPropertyException(key, ex);
				}
			}
			return (T)row;
		}

		protected static Dictionary<int, string> GetKeyMappings(DbDataReader dataReader)
		{
			return Enumerable
				.Range(0, dataReader.FieldCount)
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