using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Xdr.EmitContexts
{
	public class FieldDesc
	{
		private MemberInfo _mi;
		private Type _fieldType;

		public FieldDesc(Type ft, MemberInfo mi)
		{
			_fieldType = ft;
			_mi = mi;
			
			//TODO: check attribute combination errors
		}
		
		public MethodBuilder CreateReaded(TypeBuilder typeBuilder, FieldBuilder targetField, out ILGenerator il)
		{
			MethodBuilder mb = typeBuilder.DefineMethod(_mi.Name + "_Readed", MethodAttributes.Public, null, new Type[] { _fieldType });
			il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, targetField);
			il.Emit(OpCodes.Ldarg_1);
			EmitSet(il);
			return mb;
		}

		public void AppendCall(ILGenerator il, FieldBuilder readerField, MethodBuilder nextMethod, FieldBuilder exceptedField)
		{
			
			
//IL_000C: ldarg.0
//IL_000D: ldfld Xdr.Reader Xdr.TestDtos.SimplyInt/ReadContext._reader
//IL_0012: ldarg.0
//IL_0013: ldftn System.Void Xdr.TestDtos.SimplyInt/ReadContext.Field2_Readed(System.UInt32)
//IL_0019: newobj System.Action`1System.UInt32(System.Object,System.IntPtr)
//IL_001E: ldarg.0
//IL_001F: ldfld System.Action`1System.Exception Xdr.TestDtos.SimplyInt/ReadContext._excepted
//IL_0024: callvirt Xdr.Reader.Read(System.Action`1!!0,System.Action`1System.Exception)
//IL_0029: ret
			
			
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, readerField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, nextMethod);
			il.Emit(OpCodes.Newobj, typeof(Action<>).MakeGenericType(_fieldType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			
			//MethodInfo mi = typeof(Reader).GetMethod("Read").MakeGenericMethod(_fieldType)
			
			
			il.Emit(OpCodes.Callvirt, typeof(Reader).GetMethod("Read").MakeGenericMethod(_fieldType));
			il.Emit(OpCodes.Ret);
		}
		
		private void EmitSet(ILGenerator il)
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
	}
}

