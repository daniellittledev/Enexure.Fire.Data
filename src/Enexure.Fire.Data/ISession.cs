using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Enexure.Fire.Data
{
	public interface ISession : IDisposable
	{
		ICommand CreateCommand(DbCommand dbCommand);
		ICommand CreateCommand(string sql, params object[] parameters);
		ICommand CreateCommand(string sql, IEnumerable<object> parameters);
		ICommand CreateCommandWithParameters(string sql, IEnumerable<Parameter> parameters);
		ICommand CreateCommandWithParameters(string sql, params Parameter[] parameters);

		void Commit();
		void Rollback();
	}
}
