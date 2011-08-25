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
	public class ReadManyCacheDescription
	{
		public readonly Type Result;
		public readonly FieldInfo Instance;

		public ReadManyCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
		{
			TypeBuilder typeBuilder = modBuilder.DefineType("ReadManyCache",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
			
			GenericTypeParameterBuilder genTypeParam = typeBuilder.DefineGenericParameters("T")[0];

			FieldBuilder fb_Instance = typeBuilder.DefineField("Instance", typeof(ReadManyDelegate<>).MakeGenericType(genTypeParam),
				FieldAttributes.Public | FieldAttributes.Static);

			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, new Type[0]);
			ILGenerator ilCtor = ctor.GetILGenerator();
			
			ilCtor.Emit(OpCodes.Ldsfld, delegCacheDesc.BuildRequest);
			//ldsfld class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType> Xdr.Examples.DelegateCache::BuildRequest
			ilCtor.Emit(OpCodes.Ldtoken, genTypeParam);
			//ldtoken !T
			ilCtor.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			//call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			ilCtor.Emit(OpCodes.Ldc_I4_1); // MethodType.ReadMany
			//ldc.i4.0
			ilCtor.Emit(OpCodes.Callvirt, typeof(Action<Type, MethodType>).GetMethod("Invoke"));
			//callvirt instance void class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType>::Invoke(!0, !1)
			ilCtor.Emit(OpCodes.Ret);

			Result = typeBuilder.CreateType();
			Instance = fb_Instance;
		}
	}
}
