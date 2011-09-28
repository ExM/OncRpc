using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Xdr.EmitContexts.Fields
{
	public class Field: FieldDesc
	{
		private FieldInfo _fi;
		
		public Field(FieldInfo fi)
		{
			_fi = fi;
			
			ExtractAttributes();
		}
		
		public override MemberInfo MInfo
		{
			get
			{
				return _fi;
			}
		}
		
		public override Type FieldType
		{
			get
			{
				return _fi.FieldType;
			}
		}
		
		public override void EmitGet(ILGenerator il, FieldBuilder itemField)
		{
			il.Emit(OpCodes.Ldarg_0);
			
			if(MInfo.DeclaringType.IsValueType)
				il.Emit(OpCodes.Ldflda, itemField);
			else
				il.Emit(OpCodes.Ldfld, itemField);
			
			il.Emit(OpCodes.Ldfld, _fi);
		}
		
		protected override void EmitSet(ILGenerator il, FieldBuilder targetField)
		{
			il.Emit(OpCodes.Ldarg_0);

			if(MInfo.DeclaringType.IsValueType)
				il.Emit(OpCodes.Ldflda, targetField);
			else
				il.Emit(OpCodes.Ldfld, targetField);
			
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, _fi);
		}
	}
}

