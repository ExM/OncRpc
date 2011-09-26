using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr.EmitContexts
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
			_typeBuilder = mb.DefineType(_targetType.FullName + ".ReadContext", TypeAttributes.Public | TypeAttributes.Class);
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
			
			
			//TODO
			/*
IL_0000: ldarg.1
IL_0001: ldc.i4.1
IL_0002: bne.un IL_002A
IL_0007: ldarg.0
IL_0008: ldfld Xdr.Reader Xdr.Example.FileType/ReadContext._reader
IL_000D: ldc.i4 255
IL_0012: ldarg.0
IL_0013: ldftn System.Void Xdr.Example.FileType/ReadContext.Creator_Readed(System.String)
IL_0019: newobj System.Action`1System.String(System.Object,System.IntPtr)
IL_001E: ldarg.0
IL_001F: ldfld System.Action`1System.Exception Xdr.Example.FileType/ReadContext._excepted
IL_0024: callvirt Xdr.Reader.ReadVar(System.UInt32,System.Action`1!!0,System.Action`1System.Exception)
IL_0029: ret
IL_002A: ldarg.1
IL_002B: ldc.i4.2
IL_002C: bne.un IL_0054
IL_0031: ldarg.0
IL_0032: ldfld Xdr.Reader Xdr.Example.FileType/ReadContext._reader
IL_0037: ldc.i4 255
IL_003C: ldarg.0
IL_003D: ldftn System.Void Xdr.Example.FileType/ReadContext.Interpretor_Readed(System.String)
IL_0043: newobj System.Action`1System.String(System.Object,System.IntPtr)
IL_0048: ldarg.0
IL_0049: ldfld System.Action`1System.Exception Xdr.Example.FileType/ReadContext._excepted
IL_004E: callvirt Xdr.Reader.ReadVar(System.UInt32,System.Action`1!!0,System.Action`1System.Exception)
IL_0053: ret
IL_0054: ldarg.1
IL_0055: brtrue IL_006C
IL_005A: ldarg.0
IL_005B: ldfld System.Action`1Xdr.Example.FileType Xdr.Example.FileType/ReadContext._completed
IL_0060: ldarg.0
IL_0061: ldfld Xdr.Example.FileType Xdr.Example.FileType/ReadContext._target
IL_0066: callvirt System.Action`1Xdr.Example.FileType.Invoke(!0)
IL_006B: ret
			*/
			
			
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

		private void CreateFields()
		{
			_targetField = _typeBuilder.DefineField("_target", _targetType, FieldAttributes.Private);
			_readerField = _typeBuilder.DefineField("_reader", typeof(Reader), FieldAttributes.Private);
			_completedField = _typeBuilder.DefineField("_completed", typeof(Action<>).MakeGenericType(_targetType), FieldAttributes.Private);
			_exceptedField = _typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);
		}
	}
}

