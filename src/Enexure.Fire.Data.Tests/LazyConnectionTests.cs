using System.Data;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Enexure.Fire.Data.Tests
{
	[TestFixture]
	public class LazyConnectionTests
	{

		[Test]
		public async Task ConnectionShouldConnectWhenCommandIsExecuted()
		{
			var connection = TestDatabase.GetConnection();

			using (var session = new Session(connection)) {

				var sampleQuery = @"Select 'Test'";

				connection.State.Should().Be(ConnectionState.Closed);

				var result  = await session.CreateCommand(sampleQuery).ExecuteScalarAsync<string>();

				connection.State.Should().Be(ConnectionState.Open);

				result.Should().Be("Test");
			}

		}
	}
}
