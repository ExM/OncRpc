using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Xdr.EmitContexts.Fields
{
	public class Property: FieldDesc
	{
		private PropertyInfo _pi;
		
		public Property(PropertyInfo pi)
		{
			_pi = pi;
			
			ExtractAttributes();
		}
		
		public override MemberInfo MInfo
		{
			get
			{
				return _pi;
			}
		}
		
		public override Type FieldType
		{
			get
			{
				return _pi.PropertyType;
			}
		}
		
		public override void EmitGet(ILGenerator il)
		{
			il.Emit(OpCodes.Callvirt, _pi.GetGetMethod());
		}
		
		protected override void EmitSet(ILGenerator il, FieldBuilder targetField)
		{
			if(MInfo.DeclaringType.IsValueType)
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldflda, targetField);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Call, _pi.GetSetMethod());
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, targetField);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Callvirt, _pi.GetSetMethod());
			}
		}
	}
}

