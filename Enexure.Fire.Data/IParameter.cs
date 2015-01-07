using System.Data;

namespace Enexure.Fire.Data
{
	public interface IParameter
	{
		System.Data.DbType DbType { get; set; }

		bool IsNullable { get; }

		string ParameterName { get; set; }

		object Value { get; set; }
	}

	class Parameter : IParameter
	{
		public DbType DbType { get; set; }
		public bool IsNullable { get; private set; }
		public string ParameterName { get; set; }
		public object Value { get; set; }
	}
}