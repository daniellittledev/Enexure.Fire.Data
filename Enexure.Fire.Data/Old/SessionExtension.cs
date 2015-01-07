using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public static class SessionExtension
	{
		public static IEnumerable<T> ExecuteQuery<T>(this IDbSession session, string sql, Func<IDataReader, IEnumerable<T>> readerFunc, params object[] parameters)
		{
			return ExecuteQuery<T>(session, sql, parameters, readerFunc);
		}

		public static IEnumerable<T> ExecuteQuery<T>(this IDbSession session, string sql, IEnumerable<object> parameters, Func<IDataReader, IEnumerable<T>> readerFunc)
		{
			var command = session.CreateCommand();

			var reader = command
				.SetText(HandleParameters(command, sql))
				.SetParameters(GetParameters(command, parameters))
				.ExecuteReader();

			using (reader) {
				return readerFunc(reader);
			}
		}

		#region ExecuteScalar

		public static object ExecuteScalar(this IDbSession session, string sql, params object[] parameters)
		{
			return ExecuteScalar(session, sql, (IEnumerable<object>)parameters);
		}

		public static object ExecuteScalar(this IDbSession session, string sql, IEnumerable<object> parameters)
		{
			var command = session.CreateCommand();
			return ExecuteScalar(command, sql, GetParameters(command, parameters));
		}

		public static object ExecuteScalar(this IDbSession session, string sql, object parametersDictionaryObject)
		{
			var command = session.CreateCommand();
			return ExecuteScalar(command, sql, GetParametersFromObject(command, parametersDictionaryObject));
		}

		public static object ExecuteScalar(IDbCommand command, string sql, IEnumerable<IDbDataParameter> parameters)
		{
			return command
				.SetText(HandleParameters(command, sql))
				.SetParameters(parameters)
				.ExecuteScalar();
		}

		#endregion

		#region ExecuteScalarAsync

		public static Task<object> ExecuteScalarAsync(this IDbSession session, string sql, params object[] parameters)
		{
			return ExecuteScalarAsync(session, sql, (IEnumerable<object>)parameters);
		}

		public static Task<object> ExecuteScalarAsync(this IDbSession session, string sql, IEnumerable<object> parameters)
		{
			var command = session.CreateAsyncCommand();
			return ExecuteScalarAsync(command, sql, GetParameters(command, parameters));
		}

		public static Task<object> ExecuteScalarAsync(this IDbSession session, string sql, object parametersDictionaryObject)
		{
			var command = session.CreateAsyncCommand();
			return ExecuteScalarAsync(command, sql, GetParametersFromObject(command, parametersDictionaryObject));
		}

		public static Task<object> ExecuteScalarAsync(IDbCommandAsync command, string sql, IEnumerable<IDbDataParameter> parameters)
		{
			return command
				.SetText(HandleParameters(command, sql))
				.SetParameters(parameters)
				.ExecuteScalarAsync();
		}

		#endregion

		#region ExecuteNonQuery

		public static int ExecuteNonQuery(this IDbSession session, string sql, params object[] parameters)
		{
			return ExecuteNonQuery(session, sql, (IEnumerable<object>)parameters);
		}

		public static int ExecuteNonQuery(this IDbSession session, string sql, IEnumerable<object> parameters)
		{
			var command = session.CreateCommand();
			return ExecuteNonQuery(command, sql, GetParameters(command, parameters));
		}

		public static int ExecuteNonQuery(this IDbSession session, string sql, object parametersDictionaryObject)
		{
			var command = session.CreateCommand();
			return ExecuteNonQuery(command, sql, GetParametersFromObject(command, parametersDictionaryObject));
		}

		public static int ExecuteNonQuery(IDbCommand command, string sql, IEnumerable<IDbDataParameter> parameters)
		{
			return command
				.SetText(HandleParameters(command, sql))
				.SetParameters(parameters)
				.ExecuteNonQuery();
		}

		#endregion

		#region ExecuteNonQueryAsync

		public static Task<int> ExecuteNonQueryAsync(this IDbSession session, string sql, params object[] parameters)
		{
			return ExecuteNonQueryAsync(session, sql, (IEnumerable<object>)parameters);
		}

		public static Task<int> ExecuteNonQueryAsync(this IDbSession session, string sql, IEnumerable<object> parameters)
		{
			var command = session.CreateAsyncCommand();
			return ExecuteNonQueryAsync(command, sql, GetParameters(command, parameters));
		}

		public static Task<int> ExecuteNonQueryAsync(this IDbSession session, string sql, object parametersDictionaryObject)
		{
			var command = session.CreateAsyncCommand();
			return ExecuteNonQueryAsync(command, sql, GetParametersFromObject(command, parametersDictionaryObject));
		}

		public static Task<int> ExecuteNonQueryAsync(IDbCommandAsync command, string sql, IEnumerable<IDbDataParameter> parameters)
		{
			return command
				.SetText(HandleParameters(command, sql))
				.SetParameters(parameters)
				.ExecuteNonQueryAsync();
		}

		#endregion

		#region Helpers

		private static IEnumerable<IDbDataParameter> GetParametersFromObject(IDbCommand command, object parametersDictionaryObject)
		{
			var supportsNamedParameters = command is SqlCommand;
			var properties = parametersDictionaryObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var parameter in properties) {
				var param = command.CreateParameter();
				var value = parameter.GetValue(parametersDictionaryObject);

				if (value != null && value != DBNull.Value) {
					param.DbType = typeMap[parameter.PropertyType];
					param.Value = value;
				} else {
					param.Value = DBNull.Value;
				}

				if (supportsNamedParameters) {
					param.ParameterName = string.Format("{0}", parameter.Name);
				}

				yield return param;
			}
		}

		private static IEnumerable<IDbDataParameter> GetParameters(IDbCommand command, IEnumerable<object> parameters)
		{
			var i = 0;
			var supportsNamedParameters = command is SqlCommand;

			foreach (var parameter in parameters) {
				var param = command.CreateParameter();

				if (parameter != null && parameter != DBNull.Value) {
					param.DbType = typeMap[parameter.GetType()];
					param.Value = parameter;
				} else {
					param.Value = DBNull.Value;
				}

				if (supportsNamedParameters) {
					param.ParameterName = string.Format("p{0}", i);
					i += 1;
				}

				yield return param;
			}
		}

		private static string HandleParameters(IDbCommand command,  string text)
		{
			var supportsNamedParameters = command is SqlCommand;

			if (supportsNamedParameters) {
				var sql = new StringBuilder(text);

				var i = 0;
				for (var j = 0; j < sql.Length; j++) {
					if (sql[j] == '?') {
						sql.Replace("?", string.Format("@p{0}", i), j, 1);
						i += 1;
					}
				}

				return sql.ToString();
			}

			return text;
		}

		private static readonly Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>() {
			{typeof (byte), DbType.Byte},
			{typeof (sbyte), DbType.SByte},
			{typeof (short), DbType.Int16},
			{typeof (ushort), DbType.UInt16},
			{typeof (int), DbType.Int32},
			{typeof (uint), DbType.UInt32},
			{typeof (long), DbType.Int64},
			{typeof (ulong), DbType.UInt64},
			{typeof (float), DbType.Single},
			{typeof (double), DbType.Double},
			{typeof (decimal), DbType.Decimal},
			{typeof (bool), DbType.Boolean},
			{typeof (string), DbType.String},
			{typeof (char), DbType.StringFixedLength},
			{typeof (Guid), DbType.Guid},
			{typeof (DateTime), DbType.DateTime},
			{typeof (DateTimeOffset), DbType.DateTimeOffset},
			{typeof (byte[]), DbType.Binary},
			{typeof (byte?), DbType.Byte},
			{typeof (sbyte?), DbType.SByte},
			{typeof (short?), DbType.Int16},
			{typeof (ushort?), DbType.UInt16},
			{typeof (int?), DbType.Int32},
			{typeof (uint?), DbType.UInt32},
			{typeof (long?), DbType.Int64},
			{typeof (ulong?), DbType.UInt64},
			{typeof (float?), DbType.Single},
			{typeof (double?), DbType.Double},
			{typeof (decimal?), DbType.Decimal},
			{typeof (bool?), DbType.Boolean},
			{typeof (char?), DbType.StringFixedLength},
			{typeof (Guid?), DbType.Guid},
			{typeof (DateTime?), DbType.DateTime},
			{typeof (DateTimeOffset?), DbType.DateTimeOffset},
			{typeof (DBNull), DbType.Object}
			//{typeof(System.Data.Linq.Binary), DbType.Binary  },
		};

		#endregion

	}
}
