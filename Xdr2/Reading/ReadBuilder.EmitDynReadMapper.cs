using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr2
{
	public sealed partial class ReadBuilder
	{
		private Type EmitDynReadMapper()
		{
			TypeBuilder typeBuilder = _modBuilder.DefineType("DynReadMapper",
				TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed, typeof(ReadMapper));
			
			FieldBuilder fb_oneCacheType = DefineCacheField(typeBuilder, "_oneCacheType");
			FieldBuilder fb_fixCacheType = DefineCacheField(typeBuilder, "_fixCacheType");
			FieldBuilder fb_varCacheType = DefineCacheField(typeBuilder, "_varCacheType");

			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]);
			ILGenerator ilCtor = ctor.GetILGenerator();

			//	IL_0000:  ldarg.0 
			ilCtor.Emit(OpCodes.Ldarg_0);
			//	IL_0001:  call instance void class Xdr.BaseTranslator::'.ctor'()
			ilCtor.Emit(OpCodes.Call, typeof(ReadMapper).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null));

			//	IL_0006: ldarg.0
			ilCtor.Emit(OpCodes.Ldarg_0);
			//	IL_0007: ldftn instance void [Xdr]Xdr.BaseTranslator::AppendBuildRequest(class [mscorlib]System.Type, valuetype [Xdr]Xdr.Translating.MethodType)
			ilCtor.Emit(OpCodes.Ldftn, typeof(ReadMapper).GetMethod("AppendBuildRequest", BindingFlags.NonPublic | BindingFlags.Instance));
			//	IL_000d: newobj instance void class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype [Xdr]Xdr.Translating.MethodType>::.ctor(object, native int)
			ilCtor.Emit(OpCodes.Newobj, typeof(Action<Type, OpaqueType>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			//	IL_0012: stsfld class [mscorlib]System.Action`2<class [mscorlib]System.Type, valuetype [Xdr]Xdr.Translating.MethodType> Xdr.StaticSingletones.DelegateCache::BuildRequest
			ilCtor.Emit(OpCodes.Stsfld, _delegateCacheDescription.BuildRequest);
			
			EmitInitField(ilCtor, fb_oneCacheType, _oneCacheDescription.Result);
			EmitInitField(ilCtor, fb_fixCacheType, _fixCacheDescription.Result);
			EmitInitField(ilCtor, fb_varCacheType, _varCacheDescription.Result);
			
			// run init
			ilCtor.Emit(OpCodes.Ldarg_0);
			ilCtor.Emit(OpCodes.Call, typeof(ReadMapper).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic));

			ilCtor.Emit(OpCodes.Ret);

			EmitOverride_GetCacheType(typeBuilder, "GetOneCacheType", fb_oneCacheType);
			EmitOverride_GetCacheType(typeBuilder, "GetFixCacheType", fb_fixCacheType);
			EmitOverride_GetCacheType(typeBuilder, "GetVarCacheType", fb_varCacheType);
			
			return typeBuilder.CreateType();
		}

		private static FieldBuilder DefineCacheField(TypeBuilder typeBuilder, string name)
		{
			return typeBuilder.DefineField(name, typeof(Type),
				FieldAttributes.Private | FieldAttributes.InitOnly);
		}

		private static void EmitInitField(ILGenerator il, FieldBuilder fb, Type type)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldtoken, type);
			il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			il.Emit(OpCodes.Stfld, fb);
		}

		private void EmitOverride_GetCacheType(TypeBuilder typeBuilder, string overrideName, FieldBuilder fb_cacheType)
		{
			MethodBuilder mb = typeBuilder.DefineMethod(overrideName, MethodAttributes.Family | MethodAttributes.Virtual);
			mb.SetReturnType(typeof(Type));
			typeBuilder.DefineMethodOverride(mb,
				typeof(ReadMapper).GetMethod(overrideName, BindingFlags.NonPublic | BindingFlags.Instance));

			ILGenerator il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb_cacheType);
			il.Emit(OpCodes.Ret);
		}

		/*
		
		
		private void EmitOverride_GetCacheType(TypeBuilder typeBuilder, string overrideName, FieldBuilder fb_cacheType)
		{
			MethodBuilder mb = typeBuilder.DefineMethod(overrideName, MethodAttributes.Family | MethodAttributes.Virtual);
			mb.SetReturnType(typeof(Type));
			typeBuilder.DefineMethodOverride(mb,
				typeof(BaseTranslator).GetMethod(overrideName, BindingFlags.NonPublic | BindingFlags.Instance));

			ILGenerator il = mb.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, fb_cacheType);
			il.Emit(OpCodes.Ret);
		}

		private static void EmitOverride_ReadTMany(TypeBuilder tb, string name, GenCacheDescription readManyCacheDesc)
		{
			MethodInfo miDeclaration = typeof(BaseTranslator).GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
			
			MethodBuilder mb = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(Reader), typeof(uint), typeof(Action<>).MakeGenericType(genTypeParam), typeof(Action<Exception>));
			tb.DefineMethodOverride(mb, miDeclaration);


			FieldInfo fi = readManyCacheDesc.Instance(genTypeParam);

			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
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
			il.Emit(OpCodes.Ldarg_1); // reader
			il.Emit(OpCodes.Ldarg_2); // len
			il.Emit(OpCodes.Ldarg_3); // completed
			il.Emit(OpCodes.Ldarg_S, 4); // excepted

			//	IL_0018:  callvirt instance void class Xdr.ReadOneDelegate`1<!!T>::Invoke(class Xdr.IReader, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>)

			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(ReadManyDelegate<>).MakeGenericType(genTypeParam),
				typeof(ReadManyDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);

			il.Emit(OpCodes.Ret);
		}
		
		private void EmitOverride_ReadTOne(TypeBuilder typeBuilder)
		{
			MethodInfo miDeclaration = typeof(BaseTranslator).GetMethod("Read", BindingFlags.Public | BindingFlags.Instance);
			
			MethodBuilder mb = typeBuilder.DefineMethod("Read", MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(Reader), typeof(Action<>).MakeGenericType(genTypeParam), typeof(Action<Exception>));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);

			FieldInfo fi = TypeBuilder.GetField(_readOneCacheDescription.Result.MakeGenericType(genTypeParam),
				_readOneCacheDescription.Result.GetField("Instance"));
			
			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
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
		
		private void EmitOverride_WriteTOne(TypeBuilder typeBuilder)
		{
			MethodInfo miDeclaration = typeof(BaseTranslator).GetMethod("Write", BindingFlags.Public | BindingFlags.Instance);
			
			MethodBuilder mb = typeBuilder.DefineMethod("Write", MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(Writer), genTypeParam, typeof(Action), typeof(Action<Exception>));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);


			FieldInfo fi = TypeBuilder.GetField(_writeOneCacheDescription.Result.MakeGenericType(genTypeParam),
				_writeOneCacheDescription.Result.GetField("Instance"));
			
			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Brtrue, noBuild);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, typeof(BaseTranslator).GetMethod("BuildCaches", BindingFlags.NonPublic | BindingFlags.Instance));
			il.MarkLabel(noBuild);
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Ldarg_1); // writer
			il.Emit(OpCodes.Ldarg_2); // item
			il.Emit(OpCodes.Ldarg_3); // completed
			il.Emit(OpCodes.Ldarg_S, 4); // excepted
			MethodInfo miInvoke = TypeBuilder.GetMethod( typeof(WriteOneDelegate<>).MakeGenericType(genTypeParam),
				typeof(WriteOneDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}
		
		private void EmitOverride_WriteTMany(TypeBuilder typeBuilder, string name, GenCacheDescription writeManyCacheDesc)
		{
			MethodInfo miDeclaration = typeof(BaseTranslator).GetMethod(name, BindingFlags.Public | BindingFlags.Instance);

			MethodBuilder mb = typeBuilder.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(Writer), genTypeParam, typeof(uint), typeof(Action), typeof(Action<Exception>));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);

			FieldInfo fi = writeManyCacheDesc.Instance(genTypeParam);

			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Brtrue, noBuild);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, typeof(BaseTranslator).GetMethod("BuildCaches", BindingFlags.NonPublic | BindingFlags.Instance));
			il.MarkLabel(noBuild);
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Ldarg_1); // writer
			il.Emit(OpCodes.Ldarg_2); // items
			il.Emit(OpCodes.Ldarg_3); // len or max
			il.Emit(OpCodes.Ldarg_S, 4); // completed
			il.Emit(OpCodes.Ldarg_S, 5); // excepted
			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(WriteManyDelegate<>).MakeGenericType(genTypeParam),
				typeof(WriteManyDelegate<>).GetMethod("Invoke"));
			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}
		 * */
	}
}

