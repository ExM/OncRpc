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
		
		private bool _isOption = false;

		public FieldDesc(Type ft, MemberInfo mi)
		{
			_fieldType = ft;
			_mi = mi;
			
			var optAttr = _mi.GetAttr<OptionAttribute>();
			if(optAttr != null)
			{
				if(_fieldType.IsValueType)
					throw new InvalidOperationException("ValueType not supported Option attribute (use Nullable<> type)");
				_isOption = true;
			}
			
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
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, readerField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, nextMethod);
			il.Emit(OpCodes.Newobj, typeof(Action<>).MakeGenericType(_fieldType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			il.Emit(OpCodes.Callvirt, typeof(Reader).GetMethod(_isOption?"ReadOption":"Read").MakeGenericMethod(_fieldType));
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

