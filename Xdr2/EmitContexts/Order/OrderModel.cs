using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Xdr2.EmitContexts
{
	public class OrderModel
	{
		public List<FieldDesc> Fields {get; private set;}

		public Type BuildReadContext(ModuleBuilder mb, Type targetType)
		{
			OrderReadContextBuilder builder = new OrderReadContextBuilder(mb, targetType, this);
			return builder.Build();
		}
		
		public Type BuildWriteContext(ModuleBuilder mb, Type targetType)
		{
			OrderWriteContextBuilder builder = new OrderWriteContextBuilder(mb, targetType, this);
			return builder.Build();
		}

		public static OrderModel Create(Type t)
		{
			SortedList<uint, FieldDesc> fields = new SortedList<uint, FieldDesc>();
			
			foreach(var fi in t.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
				AppendField(fields, fi.FieldType, fi);

			foreach (var pi in t.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
				AppendField(fields, pi.PropertyType, pi);
		
			if(fields.Count == 0)
				return null;

			OrderModel result = new OrderModel();
			result.Fields = fields.Values.ToList();
			return result;
		}
		
		private static void AppendField(SortedList<uint, FieldDesc> fields, Type fieldType, MemberInfo mi)
		{
			OrderAttribute fAttr = mi.GetAttr<OrderAttribute>();
			if (fAttr == null)
				return;

			if (fields.ContainsKey(fAttr.Order))
				throw new InvalidOperationException("duplicate order " + fAttr.Order);
			
			fields.Add(fAttr.Order, FieldDesc.Create(fieldType, mi));
		}
	}
}

