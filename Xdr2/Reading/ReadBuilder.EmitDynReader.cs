using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Xdr2.Reading.Emit;

namespace Xdr2
{
	public sealed partial class ReadBuilder
	{
		private Type EmitDynReader()
		{
			TypeBuilder typeBuilder = _modBuilder.DefineType("DynReader",
				TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed, typeof(Reader));

			FieldBuilder fb_mapperInstance = typeBuilder.DefineField("Mapper", typeof(ReadMapper),
				FieldAttributes.Public | FieldAttributes.Static);
			
			ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] {typeof(IByteReader)});
			ILGenerator ilCtor = ctor.GetILGenerator();

			ilCtor.Emit(OpCodes.Ldarg_0);
			ilCtor.Emit(OpCodes.Ldarg_1); // reader
			ilCtor.Emit(OpCodes.Call, typeof(Reader).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(IByteReader) }, null));
			ilCtor.Emit(OpCodes.Ret);


			EmitOverride_ReadTOne(typeBuilder, fb_mapperInstance);
			EmitOverride_ReadTMany(typeBuilder, "ReadFix", _fixCacheDescription, fb_mapperInstance);
			EmitOverride_ReadTMany(typeBuilder, "ReadVar", _varCacheDescription, fb_mapperInstance);

			return typeBuilder.CreateType();
		}

		private void EmitOverride_ReadTOne(TypeBuilder typeBuilder, FieldInfo mapperInstance)
		{
			MethodInfo miDeclaration = typeof(Reader).GetMethod("Read", BindingFlags.Public | BindingFlags.Instance);

			MethodBuilder mb = typeBuilder.DefineMethod("Read", MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(genTypeParam);
			typeBuilder.DefineMethodOverride(mb, miDeclaration);

			FieldInfo fi = TypeBuilder.GetField(_oneCacheDescription.Result.MakeGenericType(genTypeParam),
				_oneCacheDescription.Result.GetField("Instance"));

			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Brtrue, noBuild);

			// TODO: ??? run BuildCaches
			il.Emit(OpCodes.Ldsfld, mapperInstance);
			il.Emit(OpCodes.Call, typeof(ReadMapper).GetMethod("BuildCaches", BindingFlags.Public | BindingFlags.Instance));
			//il.Emit(OpCodes.Ldarg_0);
			//il.Emit(OpCodes.Call, typeof().GetMethod("BuildCaches", BindingFlags.Public | BindingFlags.Instance));

			il.MarkLabel(noBuild);
			il.Emit(OpCodes.Ldsfld, fi); // reader

			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(ReadOneDelegate<>).MakeGenericType(genTypeParam),
				typeof(ReadOneDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}

		private static void EmitOverride_ReadTMany(TypeBuilder tb, string name, GenCacheDescription readManyCacheDesc, FieldInfo mapperInstance)
		{
			MethodInfo miDeclaration = typeof(Reader).GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
			
			MethodBuilder mb = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual);
			GenericTypeParameterBuilder genTypeParam = mb.DefineGenericParameters("T")[0];
			mb.SetReturnType(genTypeParam);
			mb.SetParameters(typeof(uint));
			tb.DefineMethodOverride(mb, miDeclaration);


			FieldInfo fi = readManyCacheDesc.Instance(genTypeParam);

			ILGenerator il = mb.GetILGenerator();
			Label noBuild = il.DefineLabel();
			il.Emit(OpCodes.Ldsfld, fi);
			il.Emit(OpCodes.Brtrue, noBuild);

			// TODO: ??? run BuildCaches
			il.Emit(OpCodes.Ldsfld, mapperInstance);
			il.Emit(OpCodes.Call, typeof(ReadMapper).GetMethod("BuildCaches", BindingFlags.Public | BindingFlags.Instance));
			//il.Emit(OpCodes.Ldarg_0);
			//il.Emit(OpCodes.Call, typeof(BaseTranslator).GetMethod("BuildCaches", BindingFlags.NonPublic | BindingFlags.Instance));

			il.MarkLabel(noBuild);
			il.Emit(OpCodes.Ldsfld, fi); // reader
			il.Emit(OpCodes.Ldarg_1); // len or max

			MethodInfo miInvoke = TypeBuilder.GetMethod(typeof(ReadManyDelegate<>).MakeGenericType(genTypeParam),
				typeof(ReadManyDelegate<>).GetMethod("Invoke"));

			il.Emit(OpCodes.Callvirt, miInvoke);
			il.Emit(OpCodes.Ret);
		}
	}
}

