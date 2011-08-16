using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Xdr.Emit
{
	public class Int32FieldDesc: IXdrFieldDesc
	{
		private FieldInfo _fi;

		public Int32FieldDesc(FieldInfo fi)
		{
			_fi = fi;
		}

		public MethodBuilder CreateRead(TypeBuilder typeBuilder, FieldBuilder targetField, out ILGenerator il)
		{
			MethodBuilder mb = typeBuilder.DefineMethod(_fi.Name + "_Readed", MethodAttributes.Private, null, new Type[] { typeof(byte[]) });
			il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, targetField);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, typeof(XdrEncoding).GetMethod("DecodeInt32", new Type[] { typeof(byte[]) }));
			il.Emit(OpCodes.Stfld, _fi);
			return mb;
		}

		public void AppendCall(ILGenerator il, FieldBuilder readerField, MethodBuilder nextMethod, FieldBuilder exceptedField, Func<Type, Delegate> dependencyResolver)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, readerField);
			il.Emit(OpCodes.Ldc_I4_4); // arg 0 = 4
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, nextMethod);
			il.Emit(OpCodes.Newobj, typeof(Action<byte[]>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			il.Emit(OpCodes.Callvirt, typeof(IByteReader).GetMethod("Read", new Type[] { typeof(uint), typeof(Action<byte[]>), typeof(Action<Exception>) }));
		}
	}
}

