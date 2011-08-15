using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Xdr.Emit
{
	public class StructureModel
	{
		public Type TargetType {get; private set;}
		private SortedList<uint, XdrFieldDesc> _fields = new SortedList<uint, XdrFieldDesc>();
		
		public StructureModel(Type t)
		{
			TargetType = t;
			
			foreach(var fi in TargetType.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
			{
				XdrFieldAttribute fAttr = GetAttr<XdrFieldAttribute>(fi);
				if(fAttr == null)
					continue;
				
				if(_fields.ContainsKey(fAttr.Order))
					throw new InvalidOperationException("duplicate order " + fAttr.Order);
				
				_fields.Add(fAttr.Order, new XdrFieldDesc(fi));
			}
		}
		
		public static T GetAttr<T>(FieldInfo fi) where T : Attribute
		{
			return fi.GetCustomAttributes(typeof(T), true)
				.FirstOrDefault() as T;
		}
	}
}

