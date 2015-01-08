namespace Enexure.Fire.Data
{
	public interface IParameter
	{
		System.Data.DbType DbType { get; set; }

		bool IsNullable { get; }

		string ParameterName { get; set; }

		object Value { get; set; }
	}
}