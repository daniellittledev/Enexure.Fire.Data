using System.Data.SqlClient;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Enexure.Fire.Data.Tests
{
	[TestFixture]
	public class ParametersTests
	{

		[Test]
		public async Task MultipleParametersShouldMatchTypes()
		{
			using (var session = new Session(TestDatabase.GetConnection())) {

				var createTableSql = @"Create Table MultipleParametersShouldMatchTypes_Table (
	[Id] int primary key,
	[String] varchar(20),
	[Boolean] bit
)";

				var insertIntoSql = @"Insert Into MultipleParametersShouldMatchTypes_Table Values (?, ?, ?)";

				session.CreateCommand(createTableSql).ExecuteNonQuery();
				session.CreateCommand(insertIntoSql, 1, "value", true).ExecuteNonQuery();

				using (var command = await session.CreateCommand("Select * From MultipleParametersShouldMatchTypes_Table").ExecuteQueryAsync())
				{
					var row = await command.SingleAsync<dynamic>();

					((int)row.Id).Should().Be(1);
					((string)row.String).Should().Be("value");
					((bool)row.Boolean).Should().Be(true);
				}
			}

		}

	    [Test] public async Task OnlyTransformUnnamedParameters()
        {
            using (var session = new Session(TestDatabase.GetConnection()))
            {

                var createTableSql = @"Create Table OnlyTransformUnnamedParameters_Table (
	[Id] int primary key,
	[String] varchar(20)
)";

                var insertIntoSql = @"Insert Into OnlyTransformUnnamedParameters_Table Values (?,'?')";

                session.CreateCommand(createTableSql).ExecuteNonQuery();
                session.CreateCommand(insertIntoSql, 1).ExecuteNonQuery();

                using (var command = await session.CreateCommand("Select * From OnlyTransformUnnamedParameters_Table").ExecuteQueryAsync())
                {
                    var row = await command.SingleAsync<dynamic>();

                    ((int)row.Id).Should().Be(1);
                    ((string)row.String).Should().Be("?");
                }
            }

        }
    }
}
