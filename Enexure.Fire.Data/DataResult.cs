using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Enexure.Fire.Data
{
	public class DataResult : DataResultBase, IDataResult
	{
		private readonly DbDataReader dataReader;
		
		public DataResult(DbDataReader dataReader)
		{
			this.dataReader = dataReader;
		}

		public IList<T> ToList<T>()
		{
			var list = new List<T>();
			var mapper = new Mapper(typeof(T));
			var length = dataReader.FieldCount;
			var keyMappings = GetKeyMappings(dataReader, length);

			while (dataReader.Read()) {
				GetValue(dataReader, mapper, length, keyMappings, list);
			}
			return list;
		}

		public T Single<T>()
		{
			return ToList<T>().Single();
		}

		public T SingleOrDefault<T>()
			where T : class
		{
			return ToList<T>().SingleOrDefault();
		}
	}

	public class DataResultAsync : DataResultBase, IDataResultAsync
	{
		private readonly Func<Task<DbDataReader>> readerProvider;

		public DataResultAsync(Func<Task<DbDataReader>> readerProvider)
		{
			this.readerProvider = readerProvider;
		}

		public async Task<IList<T>> ToListAsync<T>()
		{
			var reader = await readerProvider();

			var list = new List<T>();
			var mapper = new Mapper(typeof(T));
			var length = reader.FieldCount;
			var keyMappings = GetKeyMappings(reader, length);

			while (await reader.ReadAsync()) {
				GetValue(reader, mapper, length, keyMappings, list);
			}
			return list;
		}

		public async Task<T> SingleAsync<T>()
		{
			return (await ToListAsync<T>()).Single();
		}

		public async Task<T> SingleOrDefaultAsync<T>()
			where T : class 
		{
			return (await ToListAsync<T>()).SingleOrDefault();
		}
	}

	public abstract class DataResultBase
	{
		internal static void GetValue<T>(DbDataReader dataReader, Mapper mapper, int length, Dictionary<int, string> keyMappings, List<T> list)
		{
			var row = mapper.GetRowInstance();
			var rowMapper = mapper.GetMapper(row);

			for (var i = 0; i < length; i++) {
				rowMapper(keyMappings[i], dataReader.GetValue(i));
			}

			list.Add((T)row);
		}

		protected static Dictionary<int, string> GetKeyMappings(DbDataReader dataReader, int length)
		{
			return Enumerable
				.Range(0, length)
				.ToDictionary(index => index, dataReader.GetName);
		}
	}

	internal class Mapper
	{
		private readonly Type type;
		readonly ResultType resultType;
		readonly Dictionary<string, Action<object, object>> setters;

		private enum ResultType
		{
			Dynamic,
			Dictionary,
			Object
		}

		public Mapper(Type type)
		{
			this.type = type;

			// Dynamic is passed in as Object
			if (type == typeof(object)) {
				resultType = ResultType.Dynamic;
			} else if (type.IsAssignableFrom(typeof(IDictionary<string, object>))) {
				resultType = ResultType.Dictionary;
			} else {
				resultType = ResultType.Object;
				setters = GetSetters(type);
			}
		}

		public object GetRowInstance()
		{
			if (resultType == ResultType.Dynamic) {
				return new ExpandoObject();
			}

			if (resultType == ResultType.Dictionary) {
				return Activator.CreateInstance(type);
			}

			return Activator.CreateInstance(type);
		}

		public Action<string, object> GetMapper(object instance)
		{
			// Dynamic is passed in as Object
			if (resultType == ResultType.Dynamic || resultType == ResultType.Dictionary) {

				var dyn = (IDictionary<string, object>)new ExpandoObject();
				return dyn.Add;
			}

			return (k, v) => setters[k](instance, v);
		}

		private static Dictionary<string, Action<object, object>> GetSetters(Type type)
		{
			var setters = new Dictionary<string, Action<object, object>>();
			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.SetProperty)) {
				setters.Add(prop.Name, (o, v) => CreateSetMethod(prop)(o, v));
			}
			return setters;
		}

		private static Action<object, object> CreateSetMethod(PropertyInfo propertyInfo)
		{
			var setMethod = propertyInfo.GetSetMethod();

			//If there's no setter return null
			if (setMethod == null) {
				return null;
			}

			// Create the dynamic method
			var arguments = new Type[2];
			arguments[0] = arguments[1] = typeof(object);

			var declaringType = propertyInfo.DeclaringType;

			var setter = new DynamicMethod(String.Concat("_Set", propertyInfo.Name, "_"), typeof(void), arguments, declaringType);
			var generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, declaringType);
			generator.Emit(OpCodes.Ldarg_1);

			generator.Emit(propertyInfo.PropertyType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, propertyInfo.PropertyType);

			generator.EmitCall(OpCodes.Callvirt, setMethod, null);
			generator.Emit(OpCodes.Ret);

			// Create the delegate and return it
			return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));
		}
	}
}