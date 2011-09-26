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
		
		private bool _isMany = false;
		private bool _isFix = false;
		private uint _len = 0;
		
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
			
			var fixAttr = _mi.GetAttr<FixAttribute>();
			var varAttr = _mi.GetAttr<VarAttribute>();
			
			if(fixAttr != null && varAttr != null)
				throw new InvalidOperationException("can not use Fix and Var attributes both");
			
			if(fixAttr != null)
			{
				_isMany = true;
				_isFix = true;
				_len = fixAttr.Length;
			}
			
			if(varAttr != null)
			{
				_isMany = true;
				_isFix = false;
				_len = varAttr.MaxLength;
			}
			
			if(_isOption && _isMany)
				throw new InvalidOperationException("can not use Fix and Option attributes both or Var and Option attributes both");
		}
		
		public Type FieldType
		{
			get
			{
				return _fieldType;
			}
		}
		
		public MethodBuilder CreateWrited(TypeBuilder typeBuilder)
		{
			return typeBuilder.DefineMethod(_mi.Name + "_Writed", MethodAttributes.Private, null, new Type[0]);
		}
		
		public MethodBuilder CreateReaded(TypeBuilder typeBuilder, FieldBuilder targetField, out ILGenerator il)
		{
			MethodBuilder mb = typeBuilder.DefineMethod(_mi.Name + "_Readed", MethodAttributes.Private, null, new Type[] { _fieldType });
			il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, targetField);
			il.Emit(OpCodes.Ldarg_1);
			EmitSet(il);
			return mb;
		}
		
		public void AppendWriteRequest(ILGenerator il, FieldBuilder writerField, FieldBuilder itemField, MethodBuilder nextMethod, FieldBuilder exceptedField)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, writerField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, itemField);
			EmitGet(il); 
			
			if(_isMany)
				il.Emit(OpCodes.Ldc_I4, (int)_len);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, nextMethod);
			il.Emit(OpCodes.Newobj, typeof(Action).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			
			if(_isMany)
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isFix?"WriteFix":"WriteVar").MakeGenericMethod(_fieldType));
			else
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isOption?"WriteOption":"Write").MakeGenericMethod(_fieldType));
			
			il.Emit(OpCodes.Ret);
		}
		
		public void AppendWriteRequest(ILGenerator il, FieldBuilder writerField, FieldBuilder itemField, FieldBuilder completedField, FieldBuilder exceptedField)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, writerField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, itemField);
			EmitGet(il);
			
			if(_isMany)
				il.Emit(OpCodes.Ldc_I4, (int)_len);
			
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, completedField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			
			if(_isMany)
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isFix?"WriteFix":"WriteVar").MakeGenericMethod(_fieldType));
			else
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isOption?"WriteOption":"Write").MakeGenericMethod(_fieldType));

			il.Emit(OpCodes.Ret);
		}

		public void AppendCall(ILGenerator il, FieldBuilder readerField, MethodBuilder nextMethod, FieldBuilder exceptedField)
		{
			if(_isMany)
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, readerField);
				il.Emit(OpCodes.Ldc_I4, (int)_len);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldftn, nextMethod);
				il.Emit(OpCodes.Newobj, typeof(Action<>).MakeGenericType(_fieldType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, exceptedField);
				il.Emit(OpCodes.Callvirt, typeof(Reader).GetMethod(_isFix?"ReadFix":"ReadVar").MakeGenericMethod(_fieldType));
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, readerField);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldftn, nextMethod);
				il.Emit(OpCodes.Newobj, typeof(Action<>).MakeGenericType(_fieldType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, exceptedField);
				il.Emit(OpCodes.Callvirt, typeof(Reader).GetMethod(_isOption?"ReadOption":"Read").MakeGenericMethod(_fieldType));
			}
			il.Emit(OpCodes.Ret);
		}
		
		private void EmitGet(ILGenerator il)
		{
			FieldInfo fi = _mi as FieldInfo;
			if (fi != null)
			{
				il.Emit(OpCodes.Ldfld, fi);
				return;
			}

			PropertyInfo pi = _mi as PropertyInfo;
			if (pi != null)
			{
				il.Emit(OpCodes.Callvirt, pi.GetGetMethod());
				return;
			}

			throw new NotImplementedException("only PropertyInfo or FieldInfo");
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

