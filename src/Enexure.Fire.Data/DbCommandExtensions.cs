using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Enexure.Fire.Data
{
	public static class DbCommandExtensions
	{
		public static T SetText<T>(this T command, string text)
			where T : IDbCommand
		{
			command.CommandText = text;
			return command;
		}

		public static T SetText<T>(this T command, int commandTimeout)
			where T : IDbCommand
		{
			command.CommandTimeout = commandTimeout;
			return command;
		}

		public static T SetCommandType<T>(this T command, CommandType commandType)
			where T : IDbCommand
		{
			command.CommandType = commandType;
			return command;
		}

		public static T SetParameters<T>(this T command, IEnumerable<IDbDataParameter> parameters)
			where T : IDbCommand
		{
			foreach (var parameter in parameters) {
				command.Parameters.Add(parameter);
			}
			return command;
		}

	}
}
