using System.Data;

namespace Enexure.Fire.Data
{
	public class Parameter : IParameter
	{
		public DbType DbType { get; set; }
		public bool IsNullable { get; private set; }
		public string ParameterName { get; set; }
		public object Value { get; set; }
	}
}