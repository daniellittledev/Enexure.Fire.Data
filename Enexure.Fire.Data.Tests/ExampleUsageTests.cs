using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enexure.Fire.Data.Tests
{
	[TestClass]
	public class ExampleUsageTests
	{
		[TestMethod]
		public async Task TestMethod1()
		{

			if (false) {
				using (var unitOfWork = new UnitOfWork(null)) {

					var session = new Session(unitOfWork);

					var row = await session.CreateCommand("Select * From TableA where Id = ?", 1).ExecuteQueryAsync().SingleAsync<dynamic>();

					((int)row.Id).Should().Be(1);
				}
			}
		}
	}
}
