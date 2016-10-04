using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Enexure.Fire.Data
{
	internal class Mapper
	{
		private readonly Type type;
		readonly ResultType resultType;
		readonly IReadOnlyDictionary<string, Action<object, object>> setters;

		private enum ResultType
		{
			Dynamic,
			Dictionary,
			Object,
			Array
		}

		public Mapper(Type type)
		{
			this.type = type;

			// Dynamic is passed in as Object
			if (type == typeof(object) || type == typeof(ExpandoObject)) {
				resultType = ResultType.Dynamic;

			} else if (type.IsAssignableFrom(typeof(IDictionary<string, object>))) {
				resultType = ResultType.Dictionary;

			} else if (type.IsArray) {
				resultType = ResultType.Array;

			} else {
				resultType = ResultType.Object;
				setters = GetSetters(type);
			}
		}

		public object GetRowInstance(int columnCount)
		{
			if (resultType == ResultType.Dynamic) {
				return new ExpandoObject();
			}

			if (resultType == ResultType.Dictionary) {
				return Activator.CreateInstance(type);
			}

			if (resultType == ResultType.Array) {
				return Activator.CreateInstance(type, columnCount);
			}

			return Activator.CreateInstance(type);
		}

		public T GetRow<T>(DbDataReader dataReader)
		{
			var len = dataReader.FieldCount;
			var row = GetRowInstance(len);

			if (resultType == ResultType.Array) {

				var array = (object[])row;
				for (var i = 0; i < len; i++) {

					var rawValue = dataReader.GetValue(i);
					var value = rawValue == DBNull.Value ? null : rawValue;
					array[i] = value;
				}

				return (T)(object)array;
			} else {

				for (var i = 0; i < len; i++) {

					var rawValue = dataReader.GetValue(i);
					var value = rawValue == DBNull.Value ? null : rawValue;

					var key = dataReader.GetName(i);
					try {
						GetMapper(row)(key, value);
					} catch (Exception ex) {
						throw new CouldNotSetPropertyException(key, ex);
					}
				}

				return (T)row;
			}
		}

		public Action<string, object> GetMapper(object instance)
		{
			// Dynamic is passed in as Object
			if (resultType == ResultType.Dynamic || resultType == ResultType.Dictionary) {

				var dyn = (IDictionary<string, object>)instance;
				return dyn.Add;
			}

			return (k, v) => setters[k](instance, v);
		}

		private static readonly IDictionary<string, IReadOnlyDictionary<string, Action<object, object>>> settersCache = new ConcurrentDictionary<string, IReadOnlyDictionary<string, Action<object, object>>>();

		private static IReadOnlyDictionary<string, Action<object, object>> GetSetters(Type type)
		{
			lock (settersCache)
			{

				IReadOnlyDictionary<string, Action<object, object>> setters;
				if (settersCache.TryGetValue(type.FullName, out setters))
				{
					return setters;
				}

				setters = type
					.GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance)
					.ToDictionary(prop => prop.Name, CreateSetMethod);


				settersCache.Add(type.FullName, setters);

				return setters;
			}
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
