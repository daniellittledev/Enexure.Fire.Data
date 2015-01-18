Enexure.Fire.Data
=================
[![Build status](https://ci.appveyor.com/api/projects/status/dk0sbl83c7u19rpt/branch/master?svg=true)](https://ci.appveyor.com/project/Daniel45729/enexure-fire-data/branch/master)

> PM> Install-Package [Enexure.Fire.Data](https://www.nuget.org/packages/Enexure.Fire.Data/)

Fire.Data is a wrapper over System.Data that providies session like transaction management. Simplified command creation and simplified async usage.

	using (var session = new Session(connection)) {
	
		var row = await session
			.CreateCommand("Select * From TableA where Id = ?", 1)
			.ExecuteQueryAsync()
			.SingleAsync<dynamic>();

		session.Commit();
	}

When a command is executed the underlying connection is opened and a transaction established in a lazy manner. Upon commit or rollback the transaction is finalised and disposed appropriatly.
