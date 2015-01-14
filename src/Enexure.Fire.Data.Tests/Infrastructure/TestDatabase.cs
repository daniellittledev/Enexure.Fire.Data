using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace Enexure.Fire.Data.Tests
{
	public static class TestDatabase
	{
		private static readonly string templateMasterConnectionString = @"Server={0};Database=master;{1}";
		private static readonly string templateDatabaseConnectionString = @"Server={0};AttachDBFileName={2};Database={1};{3}";

		private static string server;
		private static string databaseName;
		private static string databasePath;
		private static string masterConnectionString;
		private static string databaseConnectionString;

		static TestDatabase()
		{
			var outputDirectory = Path.GetFullPath(ConfigurationManager.AppSettings["outputDirectory"]);

			Directory.CreateDirectory(outputDirectory);

			var appveyor = Environment.GetEnvironmentVariable("APPVEYOR");
			var isAppveyor = appveyor != null && appveyor.Equals("True", StringComparison.InvariantCultureIgnoreCase);

			server = isAppveyor ? "(local)\\SQL2012SP1" : "(LocalDB)\\v11.0";

			var auth = isAppveyor ? "User ID=sa;Password=Password12!;" : "Integrated Security=True;";

			databaseName = "TestDb";
			databasePath = Path.Combine(outputDirectory, String.Format(@"{0}.mdf", databaseName));	

			masterConnectionString = string.Format(templateMasterConnectionString, server, auth);
			databaseConnectionString = string.Format(templateDatabaseConnectionString, server, databaseName, databasePath, auth);

		}

		public static void Create()
		{
			try {
				Database.DeleteDatabase(masterConnectionString, databaseName, databasePath);
				Database.CreateDatabase(masterConnectionString, databaseName, databasePath);
			} catch (Exception ex) {
				
				throw new Exception(string.Format("Could not connect to {0}", masterConnectionString), ex);
			}
		}

		public static void Delete()
		{
			SqlConnection.ClearAllPools();
			Database.DeleteDatabase(masterConnectionString, databaseName, databasePath);
		}

		public static SqlConnection GetConnection()
		{
			return Database.GetConnection(databaseConnectionString);
		}
	}
}
