using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr2.EmitContexts
{
	public class SwitchReadContextBuilder
	{
		private SwitchModel _model;
		
		private Type _targetType;
		
		private TypeBuilder _typeBuilder;
		private FieldBuilder _targetField;
		private FieldBuilder _readerField;
		private FieldBuilder _completedField;
		private FieldBuilder _exceptedField;
		private ConstructorBuilder _constructor;
		
		private MethodBuilder _switchConvertedMethod;
		
		public SwitchReadContextBuilder(ModuleBuilder mb, Type targetType, SwitchModel model)
		{
			_targetType = targetType;
			_model = model;
			_typeBuilder = mb.DefineType(_targetType.FullName + "_ReadContext", TypeAttributes.Public | TypeAttributes.Class);
			CreateFields();
		}
		
		public Type Build()
		{
			EmitSwitchConverted();
			
			ILGenerator ilsw;
			MethodBuilder switchReadedMethod = _model.SwitchField.CreateReaded(_typeBuilder, _targetField, out ilsw);
			AppendSwitchConvertedCall(ilsw);
			
			ILGenerator ilctor = CreateConstructor();
			_model.SwitchField.AppendCall(ilctor, _readerField, switchReadedMethod, _exceptedField);
			
			CreateStaticReader();
			return _typeBuilder.CreateType();
		}
		
		private void EmitSwitchConverted()
		{
			_switchConvertedMethod = _typeBuilder.DefineMethod("Switch_Converted", MethodAttributes.Private, null, new Type[] { typeof(int) });
			ILGenerator il = _switchConvertedMethod.GetILGenerator();
			
			foreach(var pair in _model.Branches)
			{
				Label toNextBranch = il.DefineLabel();
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldc_I4, pair.Key);
				il.Emit(OpCodes.Bne_Un, toNextBranch);
				
				if(pair.Value != null)
				{
					ILGenerator ilNextMethod;
					var nextMethod = pair.Value.CreateReaded(_typeBuilder, _targetField, out ilNextMethod);
					AppendReturn(ilNextMethod);
					
					pair.Value.AppendCall(il, _readerField, nextMethod, _exceptedField);
				}
				else
				{
					AppendReturn(il);
				}
				il.MarkLabel(toNextBranch);
			}
			
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, _exceptedField);
			il.Emit(OpCodes.Ldstr, "unexpected value: `{0}'"); 
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Box, typeof(int)); 
			il.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }));
			il.Emit(OpCodes.Newobj, typeof(InvalidCastException).GetConstructor(new Type[] { typeof(string) }));
			il.Emit(OpCodes.Callvirt, typeof(Action<Exception>).GetMethod("Invoke"));
			il.Emit(OpCodes.Ret);
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
		
		private void AppendSwitchConvertedCall(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_1);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, _switchConvertedMethod);
			il.Emit(OpCodes.Newobj, typeof(Action<int>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, _exceptedField);
			il.Emit(OpCodes.Call, typeof(EnumHelper<>).MakeGenericType(_model.SwitchField.FieldType).GetMethod("EnumToInt", BindingFlags.Public | BindingFlags.Static)); //TODO: required more accurate determination
			il.Emit(OpCodes.Ret);
		}
		
		private void CreateStaticReader()
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
			
			if(_targetType.IsValueType)
			{
//IL_001B: ldarg.0
//IL_001C: ldloca.s V_0
//IL_001E: initobj Xdr.TestDtos.StructInt
//IL_0024: ldloc.0
//IL_0025: stfld Xdr.TestDtos.StructInt Xdr.TestDtos.StructInt/ReadContext._target
				LocalBuilder v0 = il.DeclareLocal(_targetType);
				
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloca_S, v0);
				il.Emit(OpCodes.Initobj,_targetType );
				il.Emit(OpCodes.Ldloc_0);
				il.Emit(OpCodes.Stfld, _targetField);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Newobj, _targetType.GetConstructor(new Type[] { }));
				il.Emit(OpCodes.Stfld, _targetField);
			}
			
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

		private void CreateFields()
		{
			_targetField = _typeBuilder.DefineField("_target", _targetType, FieldAttributes.Private);
			_readerField = _typeBuilder.DefineField("_reader", typeof(Reader), FieldAttributes.Private);
			_completedField = _typeBuilder.DefineField("_completed", typeof(Action<>).MakeGenericType(_targetType), FieldAttributes.Private);
			_exceptedField = _typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);
		}
	}
}

