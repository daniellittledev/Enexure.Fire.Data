using System;
using System.Collections.Generic;
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

				var dyn = (IDictionary<string, object>)instance;
				return dyn.Add;
			}

			return (k, v) => setters[k](instance, v);
		}

		private static Dictionary<string, Action<object, object>> GetSetters(Type type)
		{
			return type
				.GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance)
				.ToDictionary<PropertyInfo, string, Action<object, object>>(
					prop => prop.Name,
					prop => ((o, v) => CreateSetMethod(prop)(o, v)));
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