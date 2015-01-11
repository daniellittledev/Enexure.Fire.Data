using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace Enexure.Fire.Data.Tests
{
	public static class TestDatabase
	{
		public static string DatabaseName { get; private set; }
		public static string DatabasePath { get; private set; }

		static TestDatabase()
		{
			var outputDirectory = Path.GetFullPath(ConfigurationManager.AppSettings["outputDirectory"]);

			Directory.CreateDirectory(outputDirectory);

			DatabaseName = "TestDb";
			DatabasePath = Path.Combine(outputDirectory, String.Format(@"{0}.mdf", DatabaseName));	
		}

		public static SqlConnection GetConnection()
		{
			return LocalDb.GetConnection("TestDb", DatabasePath);
		}
	}
}
