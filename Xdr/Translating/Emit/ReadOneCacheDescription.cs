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
	public class ReadOneCacheDescription
	{
		public readonly Type Type;
		public readonly FieldInfo Instance;

		public ReadOneCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
		{
			TypeBuilder typeBuilder = modBuilder.DefineType("ReadOneCache",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
			
			GenericTypeParameterBuilder genTypeParam = typeBuilder.DefineGenericParameters("T")[0];

			//public static readonly ReadOneDelegate<T> Instance;
			FieldBuilder fb_Instance = typeBuilder.DefineField("Instance", typeof(ReadOneDelegate<>).MakeGenericType(genTypeParam),
				FieldAttributes.Public | FieldAttributes.Static);

			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, new Type[0]);
			ILGenerator ilCtor = ctor.GetILGenerator();
			/*
	IL_0000: ldsfld class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType> Xdr.Examples.DelegateCache::BuildRequest
	IL_0005: ldtoken !T
	IL_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
	IL_000f: ldc.i4.0
	IL_0010: callvirt instance void class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType>::Invoke(!0, !1)
	IL_0015: ret
			*/

			ilCtor.Emit(OpCodes.Ret);

			Type = typeBuilder.CreateType();
			Instance = fb_Instance;
		}
	}

	/*
	public static class ReadOneCache<T>
	{
		public static readonly ReadOneDelegate<T> Instance;
		
		static ReadOneCache()
		{
			DelegateCache.BuildRequest(typeof(T), MethodType.ReadOne);
		}
	}
	*/
}
