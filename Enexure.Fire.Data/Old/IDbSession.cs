using System.Data;

namespace Enexure.Fire.Data
{
	public interface IDbSession : IDbConnection
	{
		IDbCommandAsync CreateAsyncCommand();

		void Commit();
		void Rollback();
		void DisposeTransaction();

		IsolationLevel IsolationLevel { get; }
	}
}