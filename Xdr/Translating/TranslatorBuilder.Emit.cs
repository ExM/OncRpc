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
			
			FieldBuilder fb_readOneCacheType = DefineCacheField(typeBuilder, "_readOneCacheType");
			FieldBuilder fb_readFixCacheType = DefineCacheField(typeBuilder, "_readFixCacheType");
			FieldBuilder fb_readVarCacheType = DefineCacheField(typeBuilder, "_readVarCacheType");
			
			FieldBuilder fb_writeOneCacheType = DefineCacheField(typeBuilder, "_writeOneCacheType");
			FieldBuilder fb_writeManyCacheType = DefineCacheField(typeBuilder, "_writeManyCacheType");
			
			

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
			
			EmitInitField(ilCtor, fb_readOneCacheType, _readOneCacheDescription.Result);
			EmitInitField(ilCtor, fb_readFixCacheType, _readFixCacheDescription.Result);
			EmitInitField(ilCtor, fb_readVarCacheType, _readVarCacheDescription.Result);
			
			EmitInitField(ilCtor, fb_writeOneCacheType, _writeOneCacheDescription.Result);
			EmitInitField(ilCtor, fb_writeManyCacheType, _writeManyCacheDescription.Result);
			
			//	IL_0057: ret
			ilCtor.Emit(OpCodes.Ret);

			EmitOverride_GetCacheType(typeBuilder, "GetReadOneCacheType", fb_readOneCacheType);
			EmitOverride_ReadTOne(typeBuilder);

			EmitOverride_GetCacheType(typeBuilder, "GetReadFixCacheType", fb_readFixCacheType);
			EmitOverride_ReadTMany(typeBuilder, "ReadFix", _readFixCacheDescription);
			
			EmitOverride_GetCacheType(typeBuilder, "GetReadVarCacheType", fb_readVarCacheType);
			EmitOverride_ReadTMany(typeBuilder, "ReadVar", _readVarCacheDescription);
			
			EmitOverride_GetCacheType(typeBuilder, "GetWriteOneCacheType", fb_writeOneCacheType);
			EmitOverride_WriteTOne(typeBuilder);
			
			EmitOverride_GetCacheType(typeBuilder, "GetWriteManyCacheType", fb_writeManyCacheType);
			EmitOverride_WriteTMany(typeBuilder);
			
			return typeBuilder.CreateType();
		}
		
		private static void EmitInitField(ILGenerator il, FieldBuilder fb, Type type)
		{
			//	IL_0017: ldarg.0
			il.Emit(OpCodes.Ldarg_0);
			//	IL_0018: ldtoken Xdr.StaticSingletones.ReadOneCache`1
			il.Emit(OpCodes.Ldtoken, type);
			//	IL_001d: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			//	IL_0022: stfld class [mscorlib]System.Type Xdr.StaticSingletones.Translator::_readOneCacheType
			il.Emit(OpCodes.Stfld, fb);
		}
		
		private static FieldBuilder DefineCacheField(TypeBuilder typeBuilder, string name)
		{
			return typeBuilder.DefineField(name, typeof(Type),
				FieldAttributes.Private | FieldAttributes.InitOnly);
		}
		
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
			mb.SetParameters(typeof(IReader), typeof(uint), typeof(Action<>).MakeGenericType(genTypeParam), typeof(Action<Exception>));
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
			MethodInfo miDeclaration = null;
			foreach(var mi in typeof(BaseTranslator).GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if(mi.Name != "Write")
					continue;
				
				if(mi.GetParameters().Length == 4)
					miDeclaration = mi;
			}
			
			
			MethodBuilder mb = typeBuilder.DefineMethod("Write", MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(IWriter), genTypeParam, typeof(Action), typeof(Action<Exception>));
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
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Ldarg_S, 4);
			MethodInfo miInvoke = TypeBuilder.GetMethod( typeof(WriteOneDelegate<>).MakeGenericType(genTypeParam),
				typeof(WriteOneDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}
		
		private void EmitOverride_WriteTMany(TypeBuilder typeBuilder)
		{
			MethodInfo miDeclaration = null;
			foreach (var mi in typeof(BaseTranslator).GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (mi.Name != "Write")
					continue;

				if (mi.GetParameters().Length == 5)
					miDeclaration = mi;
			}

			MethodBuilder mb = typeBuilder.DefineMethod("Write", MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(IWriter), genTypeParam, typeof(bool), typeof(Action), typeof(Action<Exception>));
			typeBuilder.DefineMethodOverride(mb, miDeclaration);

			FieldInfo fi = _writeManyCacheDescription.Instance(genTypeParam);

			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Brtrue, noBuild);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, typeof(BaseTranslator).GetMethod("BuildCaches", BindingFlags.NonPublic | BindingFlags.Instance));
			il.MarkLabel(noBuild);
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Ldarg_S, 4);
			il.Emit(OpCodes.Ldarg_S, 5);
			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(WriteManyDelegate<>).MakeGenericType(genTypeParam),
				typeof(WriteManyDelegate<>).GetMethod("Invoke"));
			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}
	}
}

