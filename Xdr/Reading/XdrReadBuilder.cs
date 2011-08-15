using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr
{
	public static class XdrReadBuilder
	{
		private static ModuleBuilder _modBuilder;
		private static object _sync = new object();
		
		static XdrReadBuilder()
		{
			AssemblyName asmName = new AssemblyName("DynamicAssembly_XdrReading");
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
			_modBuilder = asmBuilder.DefineDynamicModule(asmName.Name);
		}
		
		public static ReaderDelegate<T> Build<T>()
		{
			if(TypeCache<T>.ReadContextExcepted != null)
				throw TypeCache<T>.ReadContextExcepted;
			
			if(TypeCache<T>.ReadContextType != null)
				return TypeCache<T>.Read;
			
			lock(_sync)
			{
				if(TypeCache<T>.ReadContextType == null)
				{
					List<Type> dependency = new List<Type>();
					try
					{
						TypeCache<T>.ReadContextType = CreateContextType(typeof(T), dependency);
					}
					catch(Exception ex)
					{
						TypeCache<T>.ReadContextExcepted = ex;
						throw;
					}
					
					try
					{
						while(dependency.Count != 0)
						{
							Type depType = dependency[0];
							dependency.RemoveAt(0);
							if(CacheTypeLoaded(depType))
								continue;
							try
							{
								Type depContType = CreateContextType(depType, dependency);
								SetCacheType(depType, depContType);
							}
							catch(Exception ex)
							{
								ErrorCacheType(depType, ex);
								throw;
							}
						}
					}
					catch(Exception ex)
					{
						//FIXME: type info
						TypeCache<T>.ReadContextExcepted = new NotImplementedException("dependency fail", ex);
						throw;
					}
				}

				return TypeCache<T>.Read;
			}
		}
		
		internal static Type CreateContextType(Type targetType, List<Type> dependency)
		{
			if (targetType == typeof(Int32))
				return typeof(ReadContexts.Integer);
			if (targetType == typeof(UInt32))
				return typeof(ReadContexts.UInteger);
			
			return CreateCustomContextType(targetType, dependency);
		}
		
		private static Type ActionType(Type t)
		{
			return typeof(Action<>).MakeGenericType(t);
		}
		
		private static Type CreateCustomContextType(Type targetType, List<Type> dependency)
		{
			TypeBuilder typeBuilder = _modBuilder.DefineType(targetType.FullName + "_ReadContext", TypeAttributes.Public | TypeAttributes.Class);
			
			FieldBuilder targetField = typeBuilder.DefineField("_target", targetType, FieldAttributes.Private);
			FieldBuilder readerField = typeBuilder.DefineField("_reader", typeof(IByteReader), FieldAttributes.Private);
			FieldBuilder completedField = typeBuilder.DefineField("_completed", ActionType(targetType), FieldAttributes.Private);
			FieldBuilder exceptedField = typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);
			
			/*
			//private void Field3_Readed(byte[] val)
			MethodBuilder mb3 = typeBuilder.DefineMethod("Field3_Readed", MethodAttributes.Private, null, new Type[] { typeof(byte[]) });
			ILGenerator ilGen3 = mb3.GetILGenerator();
			ilGen3.Emit(OpCodes.Ldarg_0);
			ilGen3.Emit(OpCodes.Ldfld, fb_target);
			ilGen3.Emit(OpCodes.Ldarg_1);
			ilGen3.Emit(OpCodes.Call, typeof(XdrEncoding).GetMethod("DecodeInt32", new Type[] { typeof(byte[]) })); // call int32 EmitTest.XdrEncoding::DecodeInt32(uint8[])
			ilGen3.Emit(OpCodes.Stfld, typeof(TreeNode).GetField("Field3")); // stfld uint8[] EmitTest.TreeNode::Field3
			ilGen3.Emit(OpCodes.Ldarg_0);
			ilGen3.Emit(OpCodes.Ldfld, fb_completed);
			ilGen3.Emit(OpCodes.Ldarg_0);
			ilGen3.Emit(OpCodes.Ldfld, fb_target);
			ilGen3.Emit(OpCodes.Callvirt, typeof(Action<TreeNode>).GetMethod("Invoke", new Type[] { typeof(TreeNode) }));//callvirt instance void [mscorlib]System.Action`1<class EmitTest.TreeNode>::Invoke(!0)
			ilGen3.Emit(OpCodes.Ret);

			//private void Field2_Readed(byte[] val)
			MethodBuilder mb2 = typeBuilder.DefineMethod("Field2_Readed", MethodAttributes.Private, null, new Type[] { typeof(byte[]) });
			ILGenerator ilGen2 = mb2.GetILGenerator();
			ilGen2.Emit(OpCodes.Ldarg_0);
			ilGen2.Emit(OpCodes.Ldfld, fb_target);
			ilGen2.Emit(OpCodes.Ldarg_1);
			ilGen2.Emit(OpCodes.Call, typeof(XdrEncoding).GetMethod("DecodeInt32", new Type[] { typeof(byte[]) })); // call int32 EmitTest.XdrEncoding::DecodeInt32(uint8[])
			ilGen2.Emit(OpCodes.Stfld, typeof(TreeNode).GetField("Field2")); // stfld uint8[] EmitTest.TreeNode::Field2
			ilGen2.Emit(OpCodes.Ldarg_0);
			ilGen2.Emit(OpCodes.Ldfld, fb_reader); //ldfld class EmitTest.IByteReader EmitTest.TreeNode_ReadContext::_reader
			ilGen2.Emit(OpCodes.Ldc_I4_4);
			ilGen2.Emit(OpCodes.Ldarg_0);
			ilGen2.Emit(OpCodes.Ldftn, mb3); //ldftn instance void EmitTest.TreeNode_ReadContext::Field3_Readed(uint8[])
			ilGen2.Emit(OpCodes.Newobj, typeof(Action<byte[]>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) })); //newobj instance void [mscorlib]System.Action`1<uint8[]>::.ctor(object, native int)
			ilGen2.Emit(OpCodes.Ldarg_0);
			ilGen2.Emit(OpCodes.Ldfld, fb_excepted);
			ilGen2.Emit(OpCodes.Callvirt, typeof(IByteReader).GetMethod("Read", new Type[] { typeof(int), typeof(Action<byte[]>), typeof(Action<Exception>) }));
			ilGen2.Emit(OpCodes.Ret);

			//private void Field2_Readed(byte[] val)
			MethodBuilder mb1 = typeBuilder.DefineMethod("Field1_Readed", MethodAttributes.Private, null, new Type[] { typeof(byte[]) });
			ILGenerator ilGen1 = mb1.GetILGenerator();
			ilGen1.Emit(OpCodes.Ldarg_0);
			ilGen1.Emit(OpCodes.Ldfld, fb_target);
			ilGen1.Emit(OpCodes.Ldarg_1);
			ilGen1.Emit(OpCodes.Call, typeof(XdrEncoding).GetMethod("DecodeInt32", new Type[] { typeof(byte[]) })); // call int32 EmitTest.XdrEncoding::DecodeInt32(uint8[])
			ilGen1.Emit(OpCodes.Stfld, typeof(TreeNode).GetField("Field1")); // stfld uint8[] EmitTest.TreeNode::Field1
			ilGen1.Emit(OpCodes.Ldarg_0);
			ilGen1.Emit(OpCodes.Ldfld, fb_reader); //ldfld class EmitTest.IByteReader EmitTest.TreeNode_ReadContext::_reader
			ilGen1.Emit(OpCodes.Ldc_I4_4);
			ilGen1.Emit(OpCodes.Ldarg_0);
			ilGen1.Emit(OpCodes.Ldftn, mb2); //ldftn instance void EmitTest.TreeNode_ReadContext::Field3_Readed(uint8[])
			ilGen1.Emit(OpCodes.Newobj, typeof(Action<byte[]>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) })); //newobj instance void [mscorlib]System.Action`1<uint8[]>::.ctor(object, native int)
			ilGen1.Emit(OpCodes.Ldarg_0);
			ilGen1.Emit(OpCodes.Ldfld, fb_excepted);
			ilGen1.Emit(OpCodes.Callvirt, typeof(IByteReader).GetMethod("Read", new Type[] { typeof(int), typeof(Action<byte[]>), typeof(Action<Exception>) }));
			ilGen1.Emit(OpCodes.Ret);

			

			ConstructorBuilder cb = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IByteReader), typeof(Action<TreeNode>), typeof(Action<Exception>) });
			ILGenerator ilGenCb = cb.GetILGenerator();

			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[] { }));
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Newobj, typeof(TreeNode).GetConstructor(new Type[] { }));
			ilGenCb.Emit(OpCodes.Stfld, fb_target); //stfld class EmitTest.TreeNode EmitTest.TreeNode_ReadContext::_target
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldarg_1);
			ilGenCb.Emit(OpCodes.Stfld, fb_reader); //stfld class EmitTest.IByteReader EmitTest.TreeNode_ReadContext::_reader
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldarg_2);
			ilGenCb.Emit(OpCodes.Stfld, fb_completed); //stfld class [mscorlib]System.Action`1<class EmitTest.TreeNode> EmitTest.TreeNode_ReadContext::_completed
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldarg_3);
			ilGenCb.Emit(OpCodes.Stfld, fb_excepted); // stfld class [mscorlib]System.Action`1<class [mscorlib]System.Exception> EmitTest.TreeNode_ReadContext::_excepted
			
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldfld, fb_reader); //ldfld class EmitTest.IByteReader EmitTest.TreeNode_ReadContext::_reader
			ilGenCb.Emit(OpCodes.Ldc_I4_4);
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldftn, mb1); //ldftn instance void EmitTest.TreeNode_ReadContext::Field3_Readed(uint8[])
			ilGenCb.Emit(OpCodes.Newobj, typeof(Action<byte[]>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) })); //newobj instance void [mscorlib]System.Action`1<uint8[]>::.ctor(object, native int)
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldfld, fb_excepted);
			ilGenCb.Emit(OpCodes.Callvirt, typeof(IByteReader).GetMethod("Read", new Type[] { typeof(int), typeof(Action<byte[]>), typeof(Action<Exception>) }));
			
			ilGenCb.Emit(OpCodes.Ret);
			*/
			
			return typeBuilder.CreateType();
		}
		
		public static bool CacheTypeLoaded(Type targetType)
		{
			Type typeCacheType = typeof(TypeCache<>).MakeGenericType(targetType);
			Exception ex = typeCacheType.GetField("ReadContextExcepted").GetValue(null) as Exception;
			if(ex != null)
				throw ex;
			return typeCacheType.GetField("ReadContextType").GetValue(null) != null;
		}
		
		private static void ErrorCacheType(Type targetType, Exception ex)
		{
			Type typeCacheType = typeof(TypeCache<>).MakeGenericType(targetType);
			typeCacheType.GetField("ReadContextExcepted").SetValue(null, ex);
		}
		
		private static void SetCacheType(Type targetType, Type readContextType)
		{
			Type typeCacheType = typeof(TypeCache<>).MakeGenericType(targetType);
			typeCacheType.GetField("ReadContextType").SetValue(null, readContextType);
		}
	}
}

