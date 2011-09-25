using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Xdr.EmitContexts
{
	public class OrderModel
	{
		public static Type BuildReadContext(ModuleBuilder mb, Type targetType, List<FieldDesc> fields)
		{
			ReadContextBuilder builder = new ReadContextBuilder(mb, targetType);
			return builder.Build(fields);
		}
		
		public static Type BuildWriteContext(ModuleBuilder mb, Type targetType, List<FieldDesc> fields)
		{
			WriteContextBuilder builder = new WriteContextBuilder(mb, targetType);
			return builder.Build(fields);
		}
		
		public static List<FieldDesc> GetFields(Type t)
		{
			SortedList<uint, FieldDesc> fields = new SortedList<uint, FieldDesc>();
			
			foreach(var fi in t.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
				AppendField(fields, fi.FieldType, fi);

			foreach (var pi in t.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
				AppendField(fields, pi.PropertyType, pi);
		
			return fields.Values.ToList();
		}
		
		private static void AppendField(SortedList<uint, FieldDesc> fields, Type fieldType, MemberInfo mi)
		{
			OrderAttribute fAttr = mi.GetAttr<OrderAttribute>();
			if (fAttr == null)
				return;

			if (fields.ContainsKey(fAttr.Order))
				throw new InvalidOperationException("duplicate order " + fAttr.Order);
			
			fields.Add(fAttr.Order, new FieldDesc(fieldType, mi));
		}
	}
}

