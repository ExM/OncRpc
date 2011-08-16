using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr.Emit
{
	public class CustomContextBuilder
	{
		private ModuleBuilder _modBuilder;
		private Func<Type, Delegate> _dependencyResolver;

		private StructureModel _model;
		private Type _targetType;
		private TypeBuilder _typeBuilder;
		private FieldBuilder _targetField;
		private FieldBuilder _readerField;
		private FieldBuilder _completedField;
		private FieldBuilder _exceptedField;

		public CustomContextBuilder(ModuleBuilder modBuilder, Func<Type, Delegate> dependencyResolver)
		{
			_modBuilder = modBuilder;
			_dependencyResolver = dependencyResolver;
		}

		private static Type ActionType(Type t)
		{
			return typeof(Action<>).MakeGenericType(t);
		}
		
		public Type Build(Type targetType)
		{
			_targetType = targetType;
			_model = new StructureModel(_targetType);

			_typeBuilder = _modBuilder.DefineType(_targetType.FullName + "_ReadContext", TypeAttributes.Public | TypeAttributes.Class);
			CreateFields();
			
			AppendCall(CreateConstructor(), 0);
			return _typeBuilder.CreateType();
		}

		private void AppendCall(ILGenerator il, int index)
		{
			MethodBuilder nextMethod = CreateRead(index);
			_model.Fields[index].AppendCall(il, _readerField, nextMethod, _exceptedField, _dependencyResolver);
			il.Emit(OpCodes.Ret);
		}

		private MethodBuilder CreateRead(int index)
		{
			ILGenerator il;
			MethodBuilder method = _model.Fields[index].CreateRead(_typeBuilder, _targetField, out il);
			if (index + 1 < _model.Fields.Count)
				AppendCall(il, index + 1);
			else
				AppendReturn(il);
			return method;
		}

		private void AppendReturn(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, _completedField);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, _targetField);
			il.Emit(OpCodes.Callvirt, ActionType(_targetType).GetMethod("Invoke", new Type[] { _targetType }));//callvirt instance void [mscorlib]System.Action`1<class EmitTest.TreeNode>::Invoke(!0)
			il.Emit(OpCodes.Ret);
		}

		private ILGenerator CreateConstructor()
		{
			ConstructorBuilder cb = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
						 new Type[] { typeof(IByteReader), typeof(Action<>).MakeGenericType(_targetType), typeof(Action<Exception>) });

			ILGenerator il = cb.GetILGenerator();

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
			_readerField = _typeBuilder.DefineField("_reader", typeof(IByteReader), FieldAttributes.Private);
			_completedField = _typeBuilder.DefineField("_completed", ActionType(_targetType), FieldAttributes.Private);
			_exceptedField = _typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);
		}
	}
}

