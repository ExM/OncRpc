using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr.EmitContexts
{
	public class ReadContextBuilder
	{
		private Type _targetType;
		
		private TypeBuilder _typeBuilder;
		private FieldBuilder _targetField;
		private FieldBuilder _readerField;
		private FieldBuilder _completedField;
		private FieldBuilder _exceptedField;
		private ConstructorBuilder _constructor;
		
		public ReadContextBuilder(ModuleBuilder mb, Type targetType)
		{
			_targetType = targetType;
			_typeBuilder = mb.DefineType(_targetType.FullName + ".ReadContext", TypeAttributes.Public | TypeAttributes.Class);
			CreateFields();
		}
		
		public Type Build(List<FieldDesc> fields)
		{
			ILGenerator il;
			MethodBuilder nextMethod;
			
			FieldDesc lastField = fields[fields.Count - 1];
			nextMethod = lastField.CreateReaded(_typeBuilder, _targetField, out il);
			AppendReturn(il);
			
			for(int i = fields.Count-2; i >= 0; i--)
			{
				var curField = fields[i];
				var nextField = fields[i+1];
				
				MethodBuilder curMethod = curField.CreateReaded(_typeBuilder, _targetField, out il);
				nextField.AppendCall(il, _readerField, nextMethod, _exceptedField);
				nextMethod = curMethod;
			}
			
			FieldDesc firstField = fields[0];
			il = CreateConstructor();
			firstField.AppendCall(il, _readerField, nextMethod, _exceptedField);
			
			CreateStaticReader();
			return _typeBuilder.CreateType();
		}
		
		public void CreateStaticReader()
		{
			MethodBuilder mb = _typeBuilder.DefineMethod("Read", MethodAttributes.Public | MethodAttributes.Static, null,
				new Type[] { typeof(Reader), typeof(Action<>).MakeGenericType(_targetType), typeof(Action<Exception>)});
			ILGenerator il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Newobj, _constructor);
			il.Emit(OpCodes.Pop);
			il.Emit(OpCodes.Ret);
		}
		
		private ILGenerator CreateConstructor()
		{
			_constructor = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
						 new Type[] { typeof(Reader), typeof(Action<>).MakeGenericType(_targetType), typeof(Action<Exception>) });

			ILGenerator il = _constructor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[] { }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Newobj, _targetType.GetConstructor(new Type[] { }));
			il.Emit(OpCodes.Stfld, _targetField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, _readerField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Stfld, _completedField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Stfld, _exceptedField);
			return il;
		}
		
		private void AppendReturn(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, _completedField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, _targetField);
			il.Emit(OpCodes.Callvirt, typeof(Action<>).MakeGenericType(_targetType).GetMethod("Invoke", new Type[] { _targetType }));
			il.Emit(OpCodes.Ret);
		}

		private void CreateFields()
		{
			_targetField = _typeBuilder.DefineField("_target", _targetType, FieldAttributes.Private);
			_readerField = _typeBuilder.DefineField("_reader", typeof(IByteReader), FieldAttributes.Private);
			_completedField = _typeBuilder.DefineField("_completed", typeof(Action<>).MakeGenericType(_targetType), FieldAttributes.Private);
			_exceptedField = _typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);
		}
	}
}

