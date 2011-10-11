using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Xdr.Emit;

namespace Xdr
{
	public sealed partial class WriteBuilder
	{
		private Type EmitDynWriter()
		{
			TypeBuilder typeBuilder = _modBuilder.DefineType("DynWriter",
				TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed, typeof(Writer));

			FieldBuilder fb_mapperInstance = typeBuilder.DefineField("Mapper", typeof(WriteMapper),
				FieldAttributes.Public | FieldAttributes.Static);

			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IByteWriter) });
			ILGenerator ilCtor = ctor.GetILGenerator();

			ilCtor.Emit(OpCodes.Ldarg_0);
			ilCtor.Emit(OpCodes.Ldarg_1); // reader
			ilCtor.Emit(OpCodes.Call, typeof(Writer).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(IByteWriter) }, null));
			ilCtor.Emit(OpCodes.Ret);

			EmitOverride_WriteTOne(typeBuilder, fb_mapperInstance);
			EmitOverride_WriteTMany(typeBuilder, "CacheWriteFix", _fixCacheDescription, fb_mapperInstance);
			EmitOverride_WriteTMany(typeBuilder, "CacheWriteVar", _varCacheDescription, fb_mapperInstance);

			return typeBuilder.CreateType();
		}

		private void EmitOverride_WriteTOne(TypeBuilder typeBuilder, FieldInfo mapperInstance)
		{
			MethodInfo miDeclaration = typeof(Writer).GetMethod("CacheWrite", BindingFlags.NonPublic | BindingFlags.Instance);

			MethodBuilder mb = typeBuilder.DefineMethod("CacheWrite", MethodAttributes.Family | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(genTypeParam);
			typeBuilder.DefineMethodOverride(mb, miDeclaration);

			FieldInfo fi = TypeBuilder.GetField(_oneCacheDescription.Result.MakeGenericType(genTypeParam),
				_oneCacheDescription.Result.GetField("Instance"));

			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Brtrue, noBuild);
			il.Emit(OpCodes.Ldsfld, mapperInstance);
			il.Emit(OpCodes.Call, typeof(WriteMapper).GetMethod("BuildCaches", BindingFlags.Public | BindingFlags.Instance));
			il.MarkLabel(noBuild);
			il.Emit(OpCodes.Ldsfld, fi); 
			il.Emit(OpCodes.Ldarg_0); // this writer
			il.Emit(OpCodes.Ldarg_1); // item

			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(WriteOneDelegate<>).MakeGenericType(genTypeParam),
				typeof(WriteOneDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}

		private static void EmitOverride_WriteTMany(TypeBuilder tb, string name, StaticCacheDescription manyCacheDesc, FieldInfo mapperInstance)
		{
			MethodInfo miDeclaration = typeof(Writer).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

			MethodBuilder mb = tb.DefineMethod(name, MethodAttributes.Family | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(null);
			mb.SetParameters(typeof(uint), genTypeParam);
			tb.DefineMethodOverride(mb, miDeclaration);

			FieldInfo fi = manyCacheDesc.Instance(genTypeParam);

			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Brtrue, noBuild);
			il.Emit(OpCodes.Ldsfld, mapperInstance);
			il.Emit(OpCodes.Call, typeof(WriteMapper).GetMethod("BuildCaches", BindingFlags.Public | BindingFlags.Instance));
			il.MarkLabel(noBuild);
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Ldarg_0);  // this writer
			il.Emit(OpCodes.Ldarg_1); // len or max
			il.Emit(OpCodes.Ldarg_2); // item

			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(WriteManyDelegate<>).MakeGenericType(genTypeParam),
				typeof(WriteManyDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}
	}
}

