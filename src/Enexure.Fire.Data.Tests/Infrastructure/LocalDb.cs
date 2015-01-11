using System;
using System.Data.SqlClient;
using System.IO;

namespace Enexure.Fire.Data.Tests
{
	static class LocalDb
	{
		private static readonly string masterConnectionString = @"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True";
		private static readonly string templateConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDBFileName={1};Initial Catalog={0};Integrated Security=True;";

		public static SqlConnection GetConnection(string databaseName, string databasePath)
		{
			return new SqlConnection(String.Format(templateConnectionString, databaseName, databasePath));
		}

		public static bool CreateDatabase(string databaseName, string dbFileName)
		{
			using (var connection = new SqlConnection(masterConnectionString)) {
				connection.Open();

				var cmd = connection.CreateCommand();
				cmd.CommandText = String.Format("Create Database {0} on (Name = N'{0}', Filename = '{1}')", databaseName, dbFileName);
				cmd.ExecuteNonQuery();
			}

			return File.Exists(dbFileName);
		}

		public static void DeleteDatabase(string databaseName, string dbFileName)
		{
			DetachDatabase(databaseName);

			if (File.Exists(dbFileName)) {
				File.Delete(dbFileName);
			}

			var dir = Path.GetDirectoryName(dbFileName);
			var ldf = Path.Combine(dir, databaseName + "_log.ldf");
			if (File.Exists(ldf)) {
				File.Delete(ldf);
			}
		}

		public static void DetachDatabase(string databaseName)
		{
			using (var connection = new SqlConnection(masterConnectionString)) {
				connection.Open();
				DetachDatabase(connection, databaseName);
			}
		}

		private static void DetachDatabase(SqlConnection connection, string databaseName)
		{
			if (DoesDbExist(connection, databaseName)) {
				var cmd = connection.CreateCommand();
				cmd.CommandText = String.Format("exec sp_detach_db '{0}'", databaseName);
				cmd.ExecuteNonQuery();
			}
		}

		private static bool DoesDbExist(SqlConnection connection, string databaseName)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = String.Format("select db_id('{0}')", databaseName);
			var exists = cmd.ExecuteScalar() != DBNull.Value;

			return exists;
		}
	}
}
