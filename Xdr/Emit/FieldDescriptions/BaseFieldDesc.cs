using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Xdr.Emit
{
	public abstract class BaseFieldDesc
	{
		protected MemberInfo _mi;

		public BaseFieldDesc(MemberInfo mi)
		{
			_mi = mi;
		}

		protected void EmitSet(ILGenerator il)
		{
			FieldInfo fi = _mi as FieldInfo;
			if (fi != null)
			{
				il.Emit(OpCodes.Stfld, fi);
				return;
			}

			PropertyInfo pi = _mi as PropertyInfo;
			if (pi != null)
			{
				il.Emit(OpCodes.Callvirt, pi.GetSetMethod());
				return;
			}

			throw new NotImplementedException("only PropertyInfo or FieldInfo");
		}

		public abstract MethodBuilder CreateRead(TypeBuilder typeBuilder, FieldBuilder targetField, out ILGenerator il);
		public abstract void AppendCall(ILGenerator il, FieldBuilder readerField, MethodBuilder nextMethod, FieldBuilder exceptedField);
	}
}

