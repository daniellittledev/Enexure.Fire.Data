using System.Data.SqlClient;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Enexure.Fire.Data.Tests
{
	[TestFixture]
	public class ScalarTests
	{

		[Test]
		public async Task SampleQueryTest()
		{
			using (var unitOfWork = new UnitOfWork(TestDatabase.GetConnection())) {

				var session = new Session(unitOfWork);

				var createTableSql = @"Create Table ScalarTable (
	Id int primary key,
	Name varchar(20)
)";

                var insertIntoSql = @"Insert Into ScalarTable Values (1, 'One')";

				session.CreateCommand(createTableSql).ExecuteNonQuery();
				session.CreateCommand(insertIntoSql).ExecuteNonQuery();

                var value = await session.CreateCommand("Select Id From ScalarTable where Id = ?", 1).ExecuteScalarAsync<int>();

                value.Should().Be(1);
			}

		}
	}
}
