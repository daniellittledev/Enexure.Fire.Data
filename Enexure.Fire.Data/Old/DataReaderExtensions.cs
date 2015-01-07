using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enexure.Fire.Linq;

namespace Enexure.Fire.Data
{
	public static class DataReaderExtensions
	{
		public static IEnumerable<IReadOnlyList<object>> GetResultsWithHeadings(this IDataReader reader)
		{
			yield return GetHeadings(reader);
			foreach (var row in GetResults(reader)) {
				yield return row;
			}
		}

		public static IReadOnlyList<object> GetHeadings(this IDataReader reader)
		{
			return GetNames(reader).ToList();
		}

		public static IEnumerable<IReadOnlyList<object>> GetResults(this IDataReader reader)
		{
			while (reader.Read()) {
				yield return GetValues(reader).ToList();
			}
		}

		private static IEnumerable<object> GetNames(IDataRecord reader)
		{
			var count = reader.FieldCount;
			for (var i = 0; i < count; i++) {
				yield return reader.GetName(i);
			}
		}

		private static IEnumerable<object> GetValues(IDataRecord reader)
		{
			var count = reader.FieldCount;
			for (var i = 0; i < count; i++) {
				yield return reader[i];
			}
		}
	}
}
