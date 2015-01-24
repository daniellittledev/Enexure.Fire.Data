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
		ICommand CreateCommand(string sql, IEnumerable<IParameter> parameters);

		void Commit();
		void Rollback();
	}
}
