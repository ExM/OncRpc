using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Xdr.Emit
{
	public class OrderModel
	{
		public static List<OrderFieldDesc> GetFields(Type t)
		{
			SortedList<uint, OrderFieldDesc> fields = new SortedList<uint, OrderFieldDesc>();
			
			foreach(var fi in t.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
				AppendField(fields, fi);

			foreach (var pi in t.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
				AppendField(fields, pi);
		
			return fields.Values.ToList();
		}
		
		private static void AppendField(SortedList<uint, OrderFieldDesc> fields, MemberInfo mi)
		{
			FieldAttribute fAttr = GetAttr<FieldAttribute>(mi);
			if (fAttr == null)
				return;

			if (fields.ContainsKey(fAttr.Order))
				throw new InvalidOperationException("duplicate order " + fAttr.Order);
			
			//TODO: check attribute combination errors

			fields.Add(fAttr.Order, new OrderFieldDesc(mi));
		}

		private static T GetAttr<T>(MemberInfo mi) where T : Attribute
		{
			return mi
				.GetCustomAttributes(typeof(T), true)
				.FirstOrDefault() as T;
		}
	}
}

