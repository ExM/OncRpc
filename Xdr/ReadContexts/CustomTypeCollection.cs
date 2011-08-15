using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr.ReadContexts
{
	public class CustomTypeCollection
	{
		private ModuleBuilder _modBuilder;
		private Dictionary<Type, Delegate> _typeMap = new Dictionary<Type, Delegate>();
		
		public CustomTypeCollection(string name)
		{
			AssemblyName asmName = new AssemblyName(name);
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
			_modBuilder = asmBuilder.DefineDynamicModule(asmName.Name);
		}
		
		public Delegate GetContext(Type t)
		{
			Delegate result;
			if(!_typeMap.TryGetValue(t, out result))
			{
				Type contextType = Build(t);
				result = Delegate.CreateDelegate(
						typeof(ReaderDelegate<>).MakeGenericType(t),
						contextType.GetMethod("Read"));
				_typeMap.Add(t, result);
			}
			return result;
		}

		private static Type ActionType(Type t)
		{
			return typeof(Action<>).MakeGenericType(t);
		}
		
		private Type Build(Type targetType)
		{
			TypeBuilder typeBuilder = _modBuilder.DefineType(targetType.FullName + "_ReadContext", TypeAttributes.Public | TypeAttributes.Class);
			
			FieldBuilder targetField = typeBuilder.DefineField("_target", targetType, FieldAttributes.Private);
			FieldBuilder readerField = typeBuilder.DefineField("_reader", typeof(IByteReader), FieldAttributes.Private);
			FieldBuilder completedField = typeBuilder.DefineField("_completed", ActionType(targetType), FieldAttributes.Private);
			FieldBuilder exceptedField = typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);
			
			ConstructorBuilder cb = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
				new Type[] { typeof(IByteReader), typeof(Action<>).MakeGenericType(targetType), typeof(Action<Exception>) });
			ILGenerator ilGenCb = cb.GetILGenerator();

			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[] { }));
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Newobj, targetType.GetConstructor(new Type[] { }));
			ilGenCb.Emit(OpCodes.Stfld, targetField);
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldarg_1);
			ilGenCb.Emit(OpCodes.Stfld, readerField);
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldarg_2);
			ilGenCb.Emit(OpCodes.Stfld, completedField);
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldarg_3);
			ilGenCb.Emit(OpCodes.Stfld, exceptedField);
			
			//TODO: begin read
			
			ilGenCb.Emit(OpCodes.Ret);
			
			
			//Public Static Method Read(reader : IByteReader, completed : Action, excepted : Action) : Void
			MethodBuilder mbRead = typeBuilder.DefineMethod("Read", MethodAttributes.Public | MethodAttributes.Static, null,
				new Type[] { typeof(IByteReader), typeof(Action<>).MakeGenericType(targetType), typeof(Action<Exception>)});
			ILGenerator ilRead = mbRead.GetILGenerator();
			ilRead.Emit(OpCodes.Ldarg_0);
			ilRead.Emit(OpCodes.Ldarg_1);
			ilRead.Emit(OpCodes.Ldarg_2);
			ilRead.Emit(OpCodes.Newobj, cb);
			ilRead.Emit(OpCodes.Pop);
			ilRead.Emit(OpCodes.Ret);
			
			return typeBuilder.CreateType();
		}
	}
}

