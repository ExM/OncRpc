using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Xdr.Emit
{
	public class StructureModel
	{
		public Type TargetType {get; private set;}
		private SortedList<uint, BaseFieldDesc> _fields = new SortedList<uint, BaseFieldDesc>();
		
		public StructureModel(Type t)
		{
			TargetType = t;
			
			foreach(var fi in TargetType.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
			{
				FieldAttribute fAttr = GetAttr<FieldAttribute>(fi);
				if(fAttr == null)
					continue;
				
				if(_fields.ContainsKey(fAttr.Order))
					throw new InvalidOperationException("duplicate order " + fAttr.Order);

				_fields.Add(fAttr.Order, FieldTypeResolve(fi.FieldType, fi));
			}

			foreach (var pi in TargetType.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
			{
				FieldAttribute fAttr = GetAttr<FieldAttribute>(pi);
				if (fAttr == null)
					continue;

				if (_fields.ContainsKey(fAttr.Order))
					throw new InvalidOperationException("duplicate order " + fAttr.Order);

				_fields.Add(fAttr.Order, FieldTypeResolve(pi.PropertyType, pi));
			}
		}

		public static BaseFieldDesc FieldTypeResolve(Type t, MemberInfo mi)
		{
			if (t == typeof(Int32))
				return new Int32FieldDesc(mi);
			if (t == typeof(UInt32))
				return new UInt32FieldDesc(mi);

			if (t == typeof(string))
			{
				VarAttribute vAttr = GetAttr<VarAttribute>(mi);
				uint max = vAttr == null ? uint.MaxValue : vAttr.MaxLength;
				return new StringFieldDesc(mi, max);
			}

			throw new NotSupportedException();
		}
		
		public static T GetAttr<T>(MemberInfo mi) where T : Attribute
		{
			return mi.GetCustomAttributes(typeof(T), true)
				.FirstOrDefault() as T;
		}

		public IList<BaseFieldDesc> Fields
		{
			get
			{
				return _fields.Values;
			}
		}
	}
}

