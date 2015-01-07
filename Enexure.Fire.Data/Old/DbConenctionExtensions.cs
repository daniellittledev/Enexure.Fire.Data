using System;
using System.Data;

namespace Enexure.Fire.Data
{
	public static class DbConenctionExtensions
	{
		public static IDbSession BeginSessionWithTransaction(this IDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
		{
			return new DbSession(connection, isolationLevel);
		}

		public static void BeginSessionWithTransaction(this IDbConnection connection, Action<IDbSession> action)
		{
			BeginSessionWithTransaction(connection, IsolationLevel.Unspecified, action);
		}

		public static void BeginSessionWithTransaction(this IDbConnection connection, IsolationLevel isolationLevel, Action<IDbSession> action)
		{
			using (var session = new DbSession(connection, isolationLevel)) {
				action(session);

				session.Commit();
			}
		}

		public static IDbSession BeginSession(this IDbConnection connection)
		{
			return new DbSession(connection);
		}
	}
}
