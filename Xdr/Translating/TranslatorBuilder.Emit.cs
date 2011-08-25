using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Xdr.Translating;
using Xdr.Translating.Emit;

namespace Xdr
{
	public sealed partial class TranslatorBuilder
	{
		private Type EmitDynTranslator()
		{
			TypeBuilder typeBuilder = _modBuilder.DefineType("DynTranslator",
				TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed, typeof(BaseTranslator));
			
			FieldBuilder fb_readOneCacheType = typeBuilder.DefineField("_readOneCacheType", typeof(Type),
				FieldAttributes.Private | FieldAttributes.InitOnly);
			FieldBuilder fb_readManyCacheType = typeBuilder.DefineField("_readManyCacheType", typeof(Type),
				FieldAttributes.Private | FieldAttributes.InitOnly);
			

			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]);
			ILGenerator ilCtor = ctor.GetILGenerator();
			
			ilCtor.Emit(OpCodes.Ldarg_0);
			ilCtor.Emit(OpCodes.Call, typeof(BaseTranslator).GetConstructor( BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null));
//	IL_0000:  ldarg.0 
//	IL_0001:  call instance void class Xdr.BaseTranslator::'.ctor'()
			
			//TODO: emit
			
//	IL_0006:  ldarg.0 
//	IL_0007:  ldftn instance void class Xdr.BaseTranslator::AppendBuildRequest(class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType)
//	IL_000d:  newobj instance void class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType>::'.ctor'(object, native int)
//	IL_0012:  stsfld class [mscorlib]System.Action`2<class [mscorlib]System.Type,valuetype Xdr.Translating.MethodType> Xdr.Examples.DelegateCache::BuildRequest

//	IL_0017:  ldarg.0 
//	IL_0018:  ldtoken Xdr.Examples.ReadOneCache`1
//	IL_001d:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
//	IL_0022:  stfld class [mscorlib]System.Type Xdr.Examples.Translator::_readOneCacheType

//	IL_0027:  ldarg.0 
//	IL_0028:  ldtoken Xdr.Examples.ReadManyCache`1
//	IL_002d:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
//	IL_0032:  stfld class [mscorlib]System.Type Xdr.Examples.Translator::_readManyCacheType

			/*
			ilCtor.Emit(OpCodes.Ldsfld, delegCacheDesc.BuildRequest);
			//ldsfld class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType> Xdr.Examples.DelegateCache::BuildRequest
			ilCtor.Emit(OpCodes.Ldtoken, genTypeParam);
			//ldtoken !T
			ilCtor.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			//call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			ilCtor.Emit(OpCodes.Ldc_I4_0); // MethodType.ReadOne
			//ldc.i4.0
			ilCtor.Emit(OpCodes.Callvirt, typeof(Action<Type, MethodType>).GetMethod("Invoke"));
			//callvirt instance void class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype Xdr.Translating.MethodType>::Invoke(!0, !1)
			*/
			ilCtor.Emit(OpCodes.Ret);
			

			EmitOverrideReadTOne(typeBuilder);
			
			
			return typeBuilder.CreateType();
		}
		
		private void EmitOverrideReadTOne(TypeBuilder typeBuilder)
		{
			
		//internal override void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted)
		//{
		//	if (ReadOneCache<T>.Instance == null)
		//		BuildCaches();
		//	ReadOneCache<T>.Instance(reader, completed, excepted);
		//}
			MethodInfo miDeclaration = null;
			foreach(var mi in typeof(BaseTranslator).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if(mi.Name != "Read")
					continue;
				
				if(mi.GetParameters().Length == 3)
					miDeclaration = mi;
			}
			
			
			MethodBuilder mb = typeBuilder.DefineMethod("Read", MethodAttributes.Family | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(IReader), typeof(Action<>).MakeGenericType(genTypeParam), typeof(Action<Exception>));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);
			
			ILGenerator il = mb.GetILGenerator();
			
			//TODO: emit
//	IL_0000:  ldsfld class Xdr.ReadOneDelegate`1<!0> class Xdr.Examples.ReadOneCache`1<!!0>::Instance
//	IL_0005:  brtrue IL_0010

//	IL_000a:  ldarg.0 
//	IL_000b:  call instance void class Xdr.BaseTranslator::BuildCaches()
//	IL_0010:  ldsfld class Xdr.ReadOneDelegate`1<!0> class Xdr.Examples.ReadOneCache`1<!!0>::Instance
//	IL_0015:  ldarg.1 
//	IL_0016:  ldarg.2 
//	IL_0017:  ldarg.3 
//	IL_0018:  callvirt instance void class Xdr.ReadOneDelegate`1<!!T>::Invoke(class Xdr.IReader, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>)
			
			il.Emit(OpCodes.Ret);
		}
	}
}

