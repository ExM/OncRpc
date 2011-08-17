using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using Xdr.ReadContexts;

namespace Xdr.Emit
{
	public class StringFieldDesc : BaseFieldDesc
	{
		private uint _maxLen;

		public StringFieldDesc(MemberInfo mi, uint maxLen)
			:base(mi)
		{
			_maxLen = maxLen;
		}

		public override MethodBuilder CreateRead(TypeBuilder typeBuilder, FieldBuilder targetField, out ILGenerator il)
		{
			MethodBuilder mb = typeBuilder.DefineMethod(_mi.Name + "_Readed", MethodAttributes.Private, null, new Type[] { typeof(string) });
			il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, targetField);
			il.Emit(OpCodes.Ldarg_1);
			EmitSet(il);
			return mb;
		}

		public override void AppendCall(ILGenerator il, FieldBuilder readerField, MethodBuilder nextMethod, FieldBuilder exceptedField, Func<Type, Delegate> dependencyResolver)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, readerField);
			il.Emit(OpCodes.Ldc_I4, (int)_maxLen);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, nextMethod);
			il.Emit(OpCodes.Newobj, typeof(Action<string>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			il.Emit(OpCodes.Newobj, typeof(StringData).GetConstructor(new Type[] { typeof(IByteReader), typeof(uint), typeof(Action<string>), typeof(Action<Exception>) }));
			il.Emit(OpCodes.Pop);
		}
	}
}

