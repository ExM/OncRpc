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

			ilCtor.Emit(OpCodes.Ldarg_0);
			ilCtor.Emit(OpCodes.Call, typeof(ReadMapper).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null));

			ilCtor.Emit(OpCodes.Ldarg_0);
			ilCtor.Emit(OpCodes.Ldftn, typeof(ReadMapper).GetMethod("AppendBuildRequest", BindingFlags.NonPublic | BindingFlags.Instance));
			ilCtor.Emit(OpCodes.Newobj, typeof(Action<Type, OpaqueType>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			ilCtor.Emit(OpCodes.Stsfld, _buildBinderDescription.BuildRequest);
			
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
	}
}

