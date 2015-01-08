using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace Enexure.Fire.Data.Tests
{
	static class LocalDb
	{
		public const string DB_DIRECTORY = "Data";

		public static SqlConnection GetLocalDB(string dbName, bool deleteIfExists = false)
		{
			try {
				var outputFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DB_DIRECTORY);
				var mdfFilename = dbName + ".mdf";
				var dbFileName = Path.Combine(outputFolder, mdfFilename);
				var logFileName = Path.Combine(outputFolder, String.Format("{0}_log.ldf", dbName));
				
				// Create Data Directory If It Doesn't Already Exist.
				if (!Directory.Exists(outputFolder)) {
					Directory.CreateDirectory(outputFolder);
				}

				// If the file exists, and we want to delete old data, remove it here and create a new database.
				if (File.Exists(dbFileName) && deleteIfExists) {
					if (File.Exists(logFileName)) File.Delete(logFileName);
					File.Delete(dbFileName);
					CreateDatabase(dbName, dbFileName);
				}
					// If the database does not already exist, create it.
				else if (!File.Exists(dbFileName)) {
					CreateDatabase(dbName, dbFileName);
				}

				// Open newly created, or old database.
				string connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;AttachDBFileName={1};Initial Catalog={0};Integrated Security=True;", dbName, dbFileName);
				var connection = new SqlConnection(connectionString);
				connection.Open();
				return connection;
			} catch {
				throw;
			}
		}

		public static bool CreateDatabase(string dbName, string dbFileName)
		{
			try {
				var connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");
				using (var connection = new SqlConnection(connectionString)) {
					connection.Open();
					SqlCommand cmd = connection.CreateCommand();


					DetachDatabase(dbName);

					cmd.CommandText = String.Format("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}')", dbName, dbFileName);
					cmd.ExecuteNonQuery();
				}

				if (File.Exists(dbFileName)) return true;
				else return false;
			} catch {
				throw;
			}
		}

		public static bool DetachDatabase(string dbName)
		{
			try {
				var connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");
				using (var connection = new SqlConnection(connectionString)) {
					connection.Open();
					SqlCommand cmd = connection.CreateCommand();
					cmd.CommandText = String.Format("exec sp_detach_db '{0}'", dbName);
					cmd.ExecuteNonQuery();

					return true;
				}
			} catch {
				return false;
			}
		}
	}
}
