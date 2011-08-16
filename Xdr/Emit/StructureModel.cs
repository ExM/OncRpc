using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Xdr.Emit
{
	public class StructureModel
	{
		public Type TargetType {get; private set;}
		private SortedList<uint, IXdrFieldDesc> _fields = new SortedList<uint, IXdrFieldDesc>();
		
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

				_fields.Add(fAttr.Order, FieldTypeResolve(fi));
			}

			foreach (var pi in TargetType.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
			{
				XdrFieldAttribute fAttr = GetAttr<XdrFieldAttribute>(pi);
				if (fAttr == null)
					continue;

				if (_fields.ContainsKey(fAttr.Order))
					throw new InvalidOperationException("duplicate order " + fAttr.Order);

				_fields.Add(fAttr.Order, PropertyTypeResolve(pi));
			}
		}

		public static IXdrFieldDesc FieldTypeResolve(FieldInfo fi)
		{
			Type t = fi.FieldType;
			if (t == typeof(Int32))
				return new Int32FieldDesc(fi);
			if (t == typeof(UInt32))
				return new UInt32FieldDesc(fi);


			throw new NotSupportedException();
		}

		public static IXdrFieldDesc PropertyTypeResolve(PropertyInfo pi)
		{
			Type t = pi.PropertyType;
			if (t == typeof(Int32))
				return new Int32PropDesc(pi);


			throw new NotSupportedException();
		}
		
		public static T GetAttr<T>(MemberInfo mi) where T : Attribute
		{
			return mi.GetCustomAttributes(typeof(T), true)
				.FirstOrDefault() as T;
		}

		public IList<IXdrFieldDesc> Fields
		{
			get
			{
				return _fields.Values;
			}
		}
	}
}

