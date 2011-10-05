using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Xdr2
{
	public static class TypeExtensions
	{
		public static T GetAttr<T>(this MemberInfo mi) where T : Attribute
		{
			return mi
				.GetCustomAttributes(typeof(T), true)
				.FirstOrDefault() as T;
		}
		
		public static IEnumerable<T> GetAttrs<T>(this MemberInfo mi) where T : Attribute
		{
			return mi
				.GetCustomAttributes(typeof(T), true)
				.Cast<T>();
		}
		
		public static Type NullableSubType(this Type type)
		{
			if (!type.IsGenericType)
				return null;
			if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
				return null;
			return type.GetGenericArguments()[0];
		}
		
		public static Type ArraySubType(this Type type)
		{
			if (!type.HasElementType)
				return null;
			Type itemType = type.GetElementType();
			if (itemType == null || itemType.MakeArrayType() != type)
				return null;
			return itemType;
		}
		
		public static Type ListSubType(this Type type)
		{
			if (!type.IsGenericType)
				return null;

			Type genericType = type.GetGenericTypeDefinition();
			if (genericType != typeof(List<>))
				return null;
			return type.GetGenericArguments()[0];
		}
	}
}
