using System.Data;

namespace Enexure.Fire.Data
{
	public class Parameter
	{
		public Parameter()
		{
			
		}

		public Parameter(object value)
		{
			Value = value;
		}

		public Parameter(string name, object value, bool isNullable = false)
		{
			ParameterName = name;
			Value = value;
		}

		public DbType DbType { get; set; }
		public bool IsNullable { get; set; }
		public string ParameterName { get; set; }
		public object Value { get; set; }
	}
}