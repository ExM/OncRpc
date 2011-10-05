using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr2.EmitContexts
{
	public class OrderWriteContextBuilder
	{
		private OrderModel _model;

		private Type _itemType;
		
		private TypeBuilder _typeBuilder;
		private FieldBuilder _itemField;
		private FieldBuilder _writerField;
		private FieldBuilder _completedField;
		private FieldBuilder _exceptedField;
		private ConstructorBuilder _constructor;

		public OrderWriteContextBuilder(ModuleBuilder mb, Type itemType, OrderModel model)
		{
			_model = model;
			_itemType = itemType;
			_typeBuilder = mb.DefineType(_itemType.FullName + "_WriteContext", TypeAttributes.Public | TypeAttributes.Class);
			CreateFields();
		}
		
		public Type Build()
		{
			List<MethodBuilder> methods = new List<MethodBuilder>();
			for (int i = 0; i < _model.Fields.Count - 1; i++)
				methods.Add(_model.Fields[i].CreateWrited(_typeBuilder));
			
			ILGenerator il = CreateConstructor();

			for (int i = 0; i < _model.Fields.Count - 1; i++)
			{
				_model.Fields[i].AppendWriteRequest(il, _writerField, _itemField, methods[i], _exceptedField);
				il = methods[i].GetILGenerator();
			}

			_model.Fields[_model.Fields.Count - 1].AppendWriteRequest(il, _writerField, _itemField, _completedField, _exceptedField);
			
			CreateStaticWriter();
			return _typeBuilder.CreateType();
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

