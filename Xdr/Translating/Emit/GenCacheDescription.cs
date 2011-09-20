using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Xdr.Translating;
using System.Reflection.Emit;
using System.Reflection;

namespace Xdr.Translating.Emit
{
	public class GenCacheDescription
	{
		public readonly Type Result;

		public GenCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc, string name, MethodType mType)
		{
			TypeBuilder typeBuilder = modBuilder.DefineType(name,
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
			
			GenericTypeParameterBuilder genTypeParam = typeBuilder.DefineGenericParameters("T")[0];
			
			Type instanceType;
			if(mType == MethodType.ReadOne)
				instanceType = typeof(ReadOneDelegate<>);
			else if(mType == MethodType.ReadFix)
				instanceType = typeof(ReadManyDelegate<>);
			else if(mType == MethodType.ReadVar)
				instanceType = typeof(ReadManyDelegate<>);
			else if(mType == MethodType.WriteOne)
				instanceType = typeof(WriteOneDelegate<>);
			else if(mType == MethodType.WriteVar)
				instanceType = typeof(WriteOneDelegate<>);
			else
				throw new ArgumentException("unknown MethodType", "mType");
				
			typeBuilder.DefineField("Instance",
				instanceType.MakeGenericType(genTypeParam),
				FieldAttributes.Public | FieldAttributes.Static);

			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, new Type[0]);
			ILGenerator il = ctor.GetILGenerator();
			
			il.Emit(OpCodes.Ldsfld, delegCacheDesc.BuildRequest);
			il.Emit(OpCodes.Ldtoken, genTypeParam);
			il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			il.Emit(OpCodes.Ldc_I4_S, (int)mType);
			il.Emit(OpCodes.Callvirt, typeof(Action<Type, MethodType>).GetMethod("Invoke"));
			il.Emit(OpCodes.Ret);

			Result = typeBuilder.CreateType();
		}
		
		public FieldInfo Instance(Type genType)
		{
			return TypeBuilder.GetField(
				Result.MakeGenericType(genType),
				Result.GetField("Instance"));
		}
	}

}
