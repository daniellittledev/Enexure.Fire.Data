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
				.SetText(HandleParameters(command, sql, parameters.Length))
				.SetParameters(GetParameters(command, parameters));

			return new Command(command, unitOfWork);
		}

		public ICommand CreateCommand(string sql, IEnumerable<object> parameters)
		{
			var command = unitOfWork.CreateCommand();
			var parametersList = parameters.ToList();

			command
				.SetText(HandleParameters(command, sql, parametersList.Count))
				.SetParameters(GetParameters(command, parametersList));

			return new Command(command, unitOfWork);
		}

		public ICommand CreateCommandWithParameters(string sql, IEnumerable<Parameter> parameters)
		{
			var command = unitOfWork.CreateCommand();
			var parametersList = parameters.ToList();

			command
				.SetText(HandleParameters(command, sql, parametersList))
				.SetParameters(parametersList.Select(x => CreateDbParameter(command.CreateParameter(), x)));

			return new Command(command, unitOfWork);
		}

		public ICommand CreateCommandWithParameters(string sql, params Parameter[] parameters)
		{
			return CreateCommandWithParameters(sql, (IEnumerable<Parameter>) parameters);
		}

		private DbParameter CreateDbParameter(DbParameter dbParameter, Parameter parameter)
		{
			dbParameter.DbType = parameter.DbType;
			dbParameter.IsNullable = parameter.IsNullable;
			dbParameter.ParameterName = parameter.ParameterName;
			dbParameter.Value = parameter.Value;

			return dbParameter;
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
			var properties = parametersDictionaryObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var parameters = properties.Select(x => new Parameter(x.GetValue(parametersDictionaryObject)));
			return GetParameters(command, parameters);
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

		private static string HandleParameters(IDbCommand command, string text, IEnumerable<Parameter> parameters)
		{
			var expectedUnnamedParameters = parameters.Count(x => string.IsNullOrEmpty(x.ParameterName));
			return HandleParameters(command, text, expectedUnnamedParameters);
		}

		private static string HandleParameters(IDbCommand command, string text, int expectedUnnamedParameters)
		{
			var supportsNamedParameters = command is SqlCommand;

			if (supportsNamedParameters)
			{
				var sql = new StringBuilder(text);

				var i = 0;
				for (var j = 0; j < sql.Length && expectedUnnamedParameters > 0; j++) {
					if (sql[j] == '?')
					{
						var parameterName = string.Format("@p{0}", i);
						sql.Replace("?", parameterName, j, 1);
						
						j += parameterName.Length - 1;
						expectedUnnamedParameters -= 1;
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
			//{typeof (DateTime), DbType.DateTime2},
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
			//{typeof (DateTime?), DbType.DateTime2},
			{typeof (DateTimeOffset?), DbType.DateTimeOffset},
			{typeof (DBNull), DbType.Object},
			{typeof(System.Data.Linq.Binary), DbType.Binary},
		};

		#endregion

	}
}