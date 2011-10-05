using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using Xdr2.EmitContexts.Fields;

namespace Xdr2.EmitContexts
{
	public abstract class FieldDesc
	{
		protected bool _isOption = false;
		
		protected bool _isMany = false;
		protected bool _isFix = false;
		protected uint _len = 0;
		

		public static FieldDesc Create(Type ft, MemberInfo mi)
		{
			FieldInfo fi = mi as FieldInfo;
			if (fi != null)
				return new Field(fi);
			
			PropertyInfo pi = mi as PropertyInfo;
			if (pi != null)
				return new Property(pi);

			throw new NotImplementedException("only PropertyInfo or FieldInfo");
		}
		
		protected FieldDesc()
		{
		}
		
		protected void ExtractAttributes()
		{
			var optAttr = MInfo.GetAttr<OptionAttribute>();
			if(optAttr != null)
			{
				if(FieldType.IsValueType)
					throw new InvalidOperationException("ValueType not supported Option attribute (use Nullable<> type)");
				_isOption = true;
			}
			
			var fixAttr = MInfo.GetAttr<FixAttribute>();
			var varAttr = MInfo.GetAttr<VarAttribute>();
			
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
		
		public abstract MemberInfo MInfo { get;}
		
		public abstract Type FieldType { get;}
		
		public MethodBuilder CreateWrited(TypeBuilder typeBuilder)
		{
			return typeBuilder.DefineMethod(MInfo.Name + "_Writed", MethodAttributes.Private, null, new Type[0]);
		}

		public MethodBuilder CreateWrited(TypeBuilder typeBuilder, int val)
		{
			return typeBuilder.DefineMethod(MInfo.Name + "_" + val.ToString() + "_Writed", MethodAttributes.Private, null, new Type[0]);
		}
		
		public MethodBuilder CreateReaded(TypeBuilder typeBuilder, FieldBuilder targetField, out ILGenerator il)
		{
			MethodBuilder mb = typeBuilder.DefineMethod(MInfo.Name + "_Readed", MethodAttributes.Private, null, new Type[] { FieldType });
			il = mb.GetILGenerator();
			EmitSet(il, targetField);
			return mb;
		}
		
		public void AppendWriteRequest(ILGenerator il, FieldBuilder writerField, FieldBuilder itemField, MethodBuilder nextMethod, FieldBuilder exceptedField)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, writerField);

			EmitGet(il, itemField); 
			
			if(_isMany)
				il.Emit(OpCodes.Ldc_I4, (int)_len);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, nextMethod);
			il.Emit(OpCodes.Newobj, typeof(Action).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			
			if(_isMany)
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isFix?"WriteFix":"WriteVar").MakeGenericMethod(FieldType));
			else
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isOption?"WriteOption":"Write").MakeGenericMethod(FieldType));
			
			il.Emit(OpCodes.Ret);
		}
		
		public void AppendWriteRequest(ILGenerator il, FieldBuilder writerField, FieldBuilder itemField, FieldBuilder completedField, FieldBuilder exceptedField)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, writerField);
			
			EmitGet(il, itemField);
			
			if(_isMany)
				il.Emit(OpCodes.Ldc_I4, (int)_len);
			
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, completedField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, exceptedField);
			
			if(_isMany)
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isFix?"WriteFix":"WriteVar").MakeGenericMethod(FieldType));
			else
				il.Emit(OpCodes.Callvirt, typeof(Writer).GetMethod(_isOption?"WriteOption":"Write").MakeGenericMethod(FieldType));

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
				il.Emit(OpCodes.Newobj, typeof(Action<>).MakeGenericType(FieldType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, exceptedField);
				il.Emit(OpCodes.Callvirt, typeof(Reader).GetMethod(_isFix?"ReadFix":"ReadVar").MakeGenericMethod(FieldType));
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, readerField);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldftn, nextMethod);
				il.Emit(OpCodes.Newobj, typeof(Action<>).MakeGenericType(FieldType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, exceptedField);
				il.Emit(OpCodes.Callvirt, typeof(Reader).GetMethod(_isOption?"ReadOption":"Read").MakeGenericMethod(FieldType));
			}
			il.Emit(OpCodes.Ret);
		}
		
		public abstract void EmitGet(ILGenerator il, FieldBuilder itemField);
		
		protected abstract void EmitSet(ILGenerator il, FieldBuilder targetField);
	}
}

