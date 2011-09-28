using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr.EmitContexts
{
	public class SwitchWriteContextBuilder
	{
		private SwitchModel _model;

		private Type _itemType;
		
		private TypeBuilder _typeBuilder;
		private FieldBuilder _itemField;
		private FieldBuilder _writerField;
		private FieldBuilder _completedField;
		private FieldBuilder _exceptedField;
		private ConstructorBuilder _constructor;

		private MethodBuilder _switchConvertedMethod;

		public SwitchWriteContextBuilder(ModuleBuilder mb, Type itemType, SwitchModel model)
		{
			_model = model;
			_itemType = itemType;
			_typeBuilder = mb.DefineType(_itemType.FullName + "_WriteContext", TypeAttributes.Public | TypeAttributes.Class);
			CreateFields();
		}
		
		public Type Build()
		{
			EmitSwitchConverted();

			ILGenerator il = CreateConstructor();
			AppendSwitchConvertedCall(il);
			
			CreateStaticWriter();
			return _typeBuilder.CreateType();
		}

		private void EmitSwitchConverted()
		{
			_switchConvertedMethod = _typeBuilder.DefineMethod("Switch_Converted", MethodAttributes.Private, null, new Type[] { typeof(int) });
			ILGenerator il = _switchConvertedMethod.GetILGenerator();

			foreach (var pair in _model.Branches)
			{
				Label toNextBranch = il.DefineLabel();
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldc_I4, pair.Key);
				il.Emit(OpCodes.Bne_Un, toNextBranch);

				if (pair.Value != null)
				{
					var nextMethod = _model.SwitchField.CreateWrited(_typeBuilder, pair.Key);
					ILGenerator ilnm = nextMethod.GetILGenerator();
					pair.Value.AppendWriteRequest(ilnm, _writerField, _itemField, _completedField, _exceptedField);

					_model.SwitchField.AppendWriteRequest(il, _writerField, _itemField, nextMethod, _exceptedField);
				}
				else
				{
					_model.SwitchField.AppendWriteRequest(il, _writerField, _itemField, _completedField, _exceptedField);
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

		private void AppendSwitchConvertedCall(ILGenerator il)
		{
			_model.SwitchField.EmitGet(il, _itemField);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldftn, _switchConvertedMethod);
			il.Emit(OpCodes.Newobj, typeof(Action<int>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, _exceptedField);
			il.Emit(OpCodes.Call, typeof(EnumHelper<>).MakeGenericType(_model.SwitchField.FieldType).GetMethod("EnumToInt", BindingFlags.Public | BindingFlags.Static)); //TODO: required more accurate determination
			il.Emit(OpCodes.Ret);
		}
		
		public void CreateStaticWriter()
		{
			MethodBuilder mb = _typeBuilder.DefineMethod("Write", MethodAttributes.Public | MethodAttributes.Static, null,
				new Type[] { typeof(Writer), _itemType, typeof(Action), typeof(Action<Exception>)});
			ILGenerator il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Newobj, _constructor);
			il.Emit(OpCodes.Pop);
			il.Emit(OpCodes.Ret);
		}
		
		private ILGenerator CreateConstructor()
		{
			_constructor = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
						 new Type[] { typeof(Writer), _itemType, typeof(Action), typeof(Action<Exception>) });

			ILGenerator il = _constructor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[] { }));
			
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, _writerField);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Stfld, _itemField);
			
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Stfld, _completedField);
			
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_S, 4);
			il.Emit(OpCodes.Stfld, _exceptedField);
			return il;
		}

		private void CreateFields()
		{
			_itemField = _typeBuilder.DefineField("_item", _itemType, FieldAttributes.Private);
			_writerField = _typeBuilder.DefineField("_writer", typeof(Writer), FieldAttributes.Private);
			_completedField = _typeBuilder.DefineField("_completed", typeof(Action), FieldAttributes.Private);
			_exceptedField = _typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);
		}
	}
}

