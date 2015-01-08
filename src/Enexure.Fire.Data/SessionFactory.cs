using System;
using System.Data.Common;

namespace Enexure.Fire.Data
{
	public class SessionFactory : ISessionFactory
	{
		private readonly string providerName;
		private readonly string connectionString;

		public SessionFactory(string providerName, string connectionString)
		{
			if (providerName == null) throw new ArgumentNullException("providerName");
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			this.providerName = providerName;
			this.connectionString = connectionString;
		}

		static DbConnection CreateDbConnection(string providerName, string connectionString)
		{
			// Assume failure.
			DbConnection connection = null;

			// Create the DbProviderFactory and DbConnection. 
			if (connectionString != null) {
				try {
					var factory = DbProviderFactories.GetFactory(providerName);

					connection = factory.CreateConnection();
					connection.ConnectionString = connectionString;
				} catch (Exception ex) {
					// Set the connection to null if it was created. 
					connection = null;
					Console.WriteLine(ex.Message);
				}
			}
			// Return the connection. 
			return connection;
		}


		public ISession CreateSession()
		{
			var connection = CreateDbConnection(providerName, connectionString);

			return new Session(new UnitOfWork(connection));
		}
	}
}
