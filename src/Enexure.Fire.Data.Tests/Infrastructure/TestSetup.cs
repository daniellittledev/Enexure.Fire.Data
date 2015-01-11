using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;

namespace Enexure.Fire.Data.Tests
{
	/// <summary>
	/// TestFixtureSetUp: Provides a config class for setting up and tearing down the test fixture.
	/// Provides the config for all TestFixture classes witin the same namespace.
	/// </summary>
	[SetUpFixture]
	public class TestFixtureSetUp
	{
		private readonly string databasePath;
		private readonly string databaseName;

		public TestFixtureSetUp()
		{
			databasePath = TestDatabase.DatabasePath;
			databaseName = TestDatabase.DatabaseName;
		}

		/// <summary>
		/// SetUp; run once before all tests in a test fixture.
		/// Run once for each TestFixture in the same namespace.
		/// </summary>
		[SetUp]
		public void RunBeforeAnyTests()
		{
			LocalDb.DeleteDatabase(databaseName, databasePath);
			LocalDb.CreateDatabase(databaseName, databasePath);
		}
		/// <summary>
		/// TearDown; run once after all tests in a test fixture.
		/// Run once for each TestFixture in the same namespace.
		/// </summary>
		[TearDown]
		public void RunAfterAnyTests()
		{
			SqlConnection.ClearAllPools();
			LocalDb.DeleteDatabase(databaseName, databasePath);
		}
	}
}
