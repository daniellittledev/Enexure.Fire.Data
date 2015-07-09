using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Enexure.Fire.Data
{
	public class Session : ISession
	{
		private readonly UnitOfWork unitOfWork;

		public Session(DbConnection connection)
			: this(new UnitOfWork(connection))
		{
		}

		public Session(UnitOfWork unitOfWork)
		{
			if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

			this.unitOfWork = unitOfWork;
		}

		public ICommand CreateCommand(DbCommand dbCommand)
		{
			return new Command(dbCommand, unitOfWork);
		}

		public ICommand CreateCommand(string sql, params object[] parameters)
		{
			var command = unitOfWork.CreateCommand();

			command
				.SetText(HandleParameters(command, sql))
				.SetParameters(GetParameters(command, parameters));

			return new Command(command, unitOfWork);
		}

		public ICommand CreateCommand(string sql, IEnumerable<object> parameters)
		{
			var command = unitOfWork.CreateCommand();

			command
				.SetText(HandleParameters(command, sql))
				.SetParameters(GetParameters(command, parameters));

			return new Command(command, unitOfWork);
		}

		public ICommand CreateCommand(string sql, IEnumerable<IParameter> parameters)
		{
			var command = unitOfWork.CreateCommand();

			command
				.SetText(HandleParameters(command, sql))
				.SetParameters(parameters.Select(x => {
					var p = command.CreateParameter();
					p.DbType = x.DbType;
					p.IsNullable = x.IsNullable;
					p.ParameterName = x.ParameterName;
					p.Value = x.Value;
					return p;
				}));

			return new Command(command, unitOfWork);
		}

		public void Commit()
		{
			unitOfWork.Commit();
		}

		public void Rollback()
		{
			unitOfWork.Rollback();
		}

		public void Dispose()
		{
			unitOfWork.Dispose();
		}

		#region Helpers

		private static IEnumerable<IDbDataParameter> GetParametersFromObject(IDbCommand command, object parametersDictionaryObject)
		{
			var supportsNamedParameters = command is SqlCommand;
			var properties = parametersDictionaryObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var parameter in properties) {
				var param = command.CreateParameter();
				var value = parameter.GetValue(parametersDictionaryObject);

				if (value != null && value != DBNull.Value) {
					param.DbType = GetDatabaseType(parameter.PropertyType);
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

		private static DbType GetDatabaseType(Type type)
		{
			DbType dbType;
			if (typeMap.TryGetValue(type, out dbType))
			{
				return dbType;
			}
			throw new KeyNotFoundException(string.Format("No database type matching {0} is supported", type.Name));
		}
		
		private static IEnumerable<IDbDataParameter> GetParameters(IDbCommand command, IEnumerable<object> parameters)
		{
			var i = 0;
			var supportsNamedParameters = command is SqlCommand;

			foreach (var parameter in parameters) {
				var param = command.CreateParameter();

				if (parameter != null && parameter != DBNull.Value) {
					param.DbType = GetDatabaseType(parameter.GetType());
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

		private static string HandleParameters(IDbCommand command, string text)
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
			{typeof (DateTime), DbType.DateTime2},
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
			{typeof (DBNull), DbType.Object},
			{typeof(System.Data.Linq.Binary), DbType.Binary},
		};

		#endregion

	}
}