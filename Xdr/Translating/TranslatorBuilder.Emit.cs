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
			FieldBuilder fb_writeOneCacheType = typeBuilder.DefineField("_writeOneCacheType", typeof(Type),
				FieldAttributes.Private | FieldAttributes.InitOnly);
			FieldBuilder fb_writeManyCacheType = typeBuilder.DefineField("_writeManyCacheType", typeof(Type),
				FieldAttributes.Private | FieldAttributes.InitOnly);
			

			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]);
			ILGenerator ilCtor = ctor.GetILGenerator();

			//	IL_0000:  ldarg.0 
			ilCtor.Emit(OpCodes.Ldarg_0);
			//	IL_0001:  call instance void class Xdr.BaseTranslator::'.ctor'()
			ilCtor.Emit(OpCodes.Call, typeof(BaseTranslator).GetConstructor( BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null));

			//	IL_0006: ldarg.0
			ilCtor.Emit(OpCodes.Ldarg_0);
			//	IL_0007: ldftn instance void [Xdr]Xdr.BaseTranslator::AppendBuildRequest(class [mscorlib]System.Type, valuetype [Xdr]Xdr.Translating.MethodType)
			ilCtor.Emit(OpCodes.Ldftn, typeof(BaseTranslator).GetMethod("AppendBuildRequest", BindingFlags.NonPublic | BindingFlags.Instance));
			//	IL_000d: newobj instance void class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype [Xdr]Xdr.Translating.MethodType>::.ctor(object, native int)
			ilCtor.Emit(OpCodes.Newobj, typeof(Action<Type, MethodType>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			//	IL_0012: stsfld class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype [Xdr]Xdr.Translating.MethodType> Xdr.StaticSingletones.DelegateCache::BuildRequest
			ilCtor.Emit(OpCodes.Stsfld, _delegateCacheDescription.BuildRequest);

			//	IL_0017: ldarg.0
			ilCtor.Emit(OpCodes.Ldarg_0);
			//	IL_0018: ldtoken Xdr.StaticSingletones.ReadOneCache`1
			ilCtor.Emit(OpCodes.Ldtoken, _readOneCacheDescription.Result);
			//	IL_001d: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			ilCtor.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			//	IL_0022: stfld class [mscorlib]System.Type Xdr.StaticSingletones.Translator::_readOneCacheType
			ilCtor.Emit(OpCodes.Stfld, fb_readOneCacheType);


			//	IL_0017: ldarg.0
			ilCtor.Emit(OpCodes.Ldarg_0);
			//	IL_0018: ldtoken Xdr.StaticSingletones.ReadManyCache`1
			ilCtor.Emit(OpCodes.Ldtoken, _readManyCacheDescription.Result);
			//	IL_001d: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			ilCtor.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			//	IL_0022: stfld class [mscorlib]System.Type Xdr.StaticSingletones.Translator::_readManyCacheType
			ilCtor.Emit(OpCodes.Stfld, fb_readManyCacheType);

			//	IL_0037: ldarg.0
			//	IL_0038: ldtoken Xdr.StaticSingletones.WriteOneCache`1
			//	IL_003d: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			//	IL_0042: stfld class [mscorlib]System.Type Xdr.StaticSingletones.Translator::_writeOneCacheType

			//	IL_0047: ldarg.0
			//	IL_0048: ldtoken Xdr.StaticSingletones.WriteManyCache`1
			//	IL_004d: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			//	IL_0052: stfld class [mscorlib]System.Type Xdr.StaticSingletones.Translator::_writeManyCacheType

			//	IL_0057: ret
			ilCtor.Emit(OpCodes.Ret);

			EmitOverride_GetReadOneCacheType(typeBuilder, fb_readOneCacheType);
			EmitOverride_ReadTOne(typeBuilder);

			EmitOverride_GetReadManyCacheType(typeBuilder, fb_readManyCacheType);
			EmitOverride_ReadTMany(typeBuilder);
			
			return typeBuilder.CreateType();
		}

		private void EmitOverride_GetReadManyCacheType(TypeBuilder typeBuilder, FieldBuilder fb_readManyCacheType)
		{
			MethodInfo miDeclaration = typeof(BaseTranslator).GetMethod("GetReadManyCacheType", BindingFlags.NonPublic | BindingFlags.Instance);

			MethodBuilder mb = typeBuilder.DefineMethod("GetReadManyCacheType", MethodAttributes.Family | MethodAttributes.Virtual);
			mb.SetReturnType(typeof(Type));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);

			ILGenerator il = mb.GetILGenerator();
			//	IL_0000: ldarg.0
			il.Emit(OpCodes.Ldarg_0);
			//	IL_0001: ldfld class [mscorlib]System.Type Xdr.StaticSingletones.Translator::_readOneCacheType
			il.Emit(OpCodes.Ldfld, fb_readManyCacheType);
			//	IL_0006: ret
			il.Emit(OpCodes.Ret);
		}

		private void EmitOverride_ReadTMany(TypeBuilder typeBuilder)
		{
			MethodInfo miDeclaration = null;
			foreach (var mi in typeof(BaseTranslator).GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (mi.Name != "Read")
					continue;

				if (mi.GetParameters().Length == 5)
					miDeclaration = mi;
			}


			MethodBuilder mb = typeBuilder.DefineMethod("Read", MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(IReader), typeof(uint), typeof(bool), typeof(Action<>).MakeGenericType(genTypeParam), typeof(Action<Exception>));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);


			FieldInfo fi = TypeBuilder.GetField(_readManyCacheDescription.Result.MakeGenericType(genTypeParam),
				_readManyCacheDescription.Result.GetField("Instance"));


			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			//TODO: emit
			//	IL_0000:  ldsfld class Xdr.ReadOneDelegate`1<!0> class Xdr.Examples.ReadOneCache`1<!!0>::Instance
			il.Emit(OpCodes.Ldsfld, fi);
			//	IL_0005:  brtrue IL_0010
			il.Emit(OpCodes.Brtrue, noBuild);
			//	IL_000a:  ldarg.0 
			il.Emit(OpCodes.Ldarg_0);
			//	IL_000b:  call instance void class Xdr.BaseTranslator::BuildCaches()
			il.Emit(OpCodes.Call, typeof(BaseTranslator).GetMethod("BuildCaches", BindingFlags.NonPublic | BindingFlags.Instance));

			il.MarkLabel(noBuild);
			//	IL_0010:  ldsfld class Xdr.ReadOneDelegate`1<!0> class Xdr.Examples.ReadOneCache`1<!!0>::Instance
			il.Emit(OpCodes.Ldsfld, fi);

			//	IL_0015:  ldarg.1 
			il.Emit(OpCodes.Ldarg_1);
			//	IL_0016:  ldarg.2 
			il.Emit(OpCodes.Ldarg_2);
			//	IL_0017:  ldarg.3 
			il.Emit(OpCodes.Ldarg_3);
			
			// ldarg.s completed
			il.Emit(OpCodes.Ldarg_S, 4);
			// ldarg.s excepted
			il.Emit(OpCodes.Ldarg_S, 5);


			//	IL_0018:  callvirt instance void class Xdr.ReadOneDelegate`1<!!T>::Invoke(class Xdr.IReader, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>)

			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(ReadManyDelegate<>).MakeGenericType(genTypeParam),
				typeof(ReadManyDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);

			il.Emit(OpCodes.Ret);
		}

		private void EmitOverride_GetReadOneCacheType(TypeBuilder typeBuilder, FieldBuilder fb_readOneCacheType)
		{
			MethodInfo miDeclaration = typeof(BaseTranslator).GetMethod("GetReadOneCacheType", BindingFlags.NonPublic | BindingFlags.Instance);

			MethodBuilder mb = typeBuilder.DefineMethod("GetReadOneCacheType", MethodAttributes.Family | MethodAttributes.Virtual);
			mb.SetReturnType(typeof(Type));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);

			ILGenerator il = mb.GetILGenerator();
			//	IL_0000: ldarg.0
			il.Emit(OpCodes.Ldarg_0);
			//	IL_0001: ldfld class [mscorlib]System.Type Xdr.StaticSingletones.Translator::_readOneCacheType
			il.Emit(OpCodes.Ldfld, fb_readOneCacheType);
			//	IL_0006: ret
			il.Emit(OpCodes.Ret);
		}
		
		private void EmitOverride_ReadTOne(TypeBuilder typeBuilder)
		{
			MethodInfo miDeclaration = null;
			foreach(var mi in typeof(BaseTranslator).GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if(mi.Name != "Read")
					continue;
				
				if(mi.GetParameters().Length == 3)
					miDeclaration = mi;
			}
			
			
			MethodBuilder mb = typeBuilder.DefineMethod("Read", MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(IReader), typeof(Action<>).MakeGenericType(genTypeParam), typeof(Action<Exception>));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);


			FieldInfo fi = TypeBuilder.GetField(_readOneCacheDescription.Result.MakeGenericType(genTypeParam),
				_readOneCacheDescription.Result.GetField("Instance"));

			
			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			//TODO: emit
//	IL_0000:  ldsfld class Xdr.ReadOneDelegate`1<!0> class Xdr.Examples.ReadOneCache`1<!!0>::Instance
			il.Emit(OpCodes.Ldsfld, fi);
//	IL_0005:  brtrue IL_0010
			il.Emit(OpCodes.Brtrue, noBuild);
//	IL_000a:  ldarg.0 
			il.Emit(OpCodes.Ldarg_0);
//	IL_000b:  call instance void class Xdr.BaseTranslator::BuildCaches()
			il.Emit(OpCodes.Call, typeof(BaseTranslator).GetMethod("BuildCaches", BindingFlags.NonPublic | BindingFlags.Instance));

			il.MarkLabel(noBuild);
//	IL_0010:  ldsfld class Xdr.ReadOneDelegate`1<!0> class Xdr.Examples.ReadOneCache`1<!!0>::Instance
			il.Emit(OpCodes.Ldsfld, fi);
//	IL_0015:  ldarg.1 
			il.Emit(OpCodes.Ldarg_1);
//	IL_0016:  ldarg.2 
			il.Emit(OpCodes.Ldarg_2);
//	IL_0017:  ldarg.3 
			il.Emit(OpCodes.Ldarg_3);
//	IL_0018:  callvirt instance void class Xdr.ReadOneDelegate`1<!!T>::Invoke(class Xdr.IReader, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>)

			MethodInfo miInvoke = TypeBuilder.GetMethod( typeof(ReadOneDelegate<>).MakeGenericType(genTypeParam),
				typeof(ReadOneDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);

			il.Emit(OpCodes.Ret);
		}
	}
}

