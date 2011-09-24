using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using Xdr;

namespace EmitTest
{
	class Program
	{
		static void Main(string[] args)
		{
			/*
			Type t1 = Create();

			object o1 = Activator.CreateInstance(t1, new ByteReader(), (Action<TreeNode>)Completed, (Action<Exception>)Excepted);

			Type t2 = Create();

			object o2 = Activator.CreateInstance(t2, new ByteReader(), (Action<TreeNode>)Completed, (Action<Exception>)Excepted);
			 * */


			Type t1 = CreateTypeCache();
		}

		public static void Completed(TreeNode val)
		{

		}

		public static void Excepted(Exception err)
		{

		}


		public static Type CreateTypeCache()
		{
			AssemblyName asmName = new AssemblyName("DynamicAssembly");
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("TypeCache.dll", "TypeCache.dll");

			TypeBuilder typeBuilder = modBuilder.DefineType("TypeCache", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);

			GenericTypeParameterBuilder gType = typeBuilder.DefineGenericParameters("T")[0];

			//public static Action<IByteReader, Action<T>, Action<Exception>> Read = null;
			FieldBuilder fb_read = typeBuilder.DefineField("Read",
				typeof(Action<,,>).MakeGenericType(
					typeof(IByteReader),
					typeof(Action<>).MakeGenericType(gType),
					typeof(Action<Exception>)
				),
				FieldAttributes.Public | FieldAttributes.Static);

			//public static Action<IByteReader, uint, bool, Action<T>, Action<Exception>> ReadArray = null;
			FieldBuilder fb_readArray = typeBuilder.DefineField("ReadArray",
				typeof(Action<,,,,>).MakeGenericType(
					typeof(IByteReader),
					typeof(uint),
					typeof(bool),
					typeof(Action<>).MakeGenericType(gType), // Action<T>
					typeof(Action<Exception>)
				),
				FieldAttributes.Public | FieldAttributes.Static);


			MethodBuilder mb_unknownType = typeBuilder.DefineMethod("UnknownType", MethodAttributes.Private | MethodAttributes.Static, typeof(Exception), new Type[0]);
			ILGenerator il_unknownType = mb_unknownType.GetILGenerator();
			LocalBuilder lb_msg = il_unknownType.DeclareLocal(typeof(string));
			il_unknownType.Emit(OpCodes.Ldstr, "unknown type `{0}'");
			il_unknownType.Emit(OpCodes.Ldtoken, gType);
			il_unknownType.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			il_unknownType.Emit(OpCodes.Callvirt, typeof(Type).GetProperty("FullName").GetGetMethod());
			il_unknownType.Emit(OpCodes.Call, typeof(String).GetMethod("Format", new Type[]{typeof(string), typeof(object)}));
			il_unknownType.Emit(OpCodes.Stloc_0);
			il_unknownType.Emit(OpCodes.Ldloc_0);
			il_unknownType.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new Type[]{typeof(string)}));
			il_unknownType.Emit(OpCodes.Ret);



		//private static void ReadStub(IByteReader reader, Action<T> completed, Action<Exception> excepted)
		//{
		//	excepted(UnknownType());
		//}

		/*
	IL_0000: ldarg.2
	IL_0001: call class [mscorlib]System.Exception class EmitTest.TypeCacheExample`1<!T>::UnknownType()
	IL_0006: callvirt instance void class [mscorlib]System.Action`1<class [mscorlib]System.Exception>::Invoke(!0)
	IL_000b: ret
		*/



			
			Type type = typeBuilder.CreateType();

			asmBuilder.Save("TypeCache.dll");
			return type;
		}


		public static Type Create()
		{
			AssemblyName asmName = new AssemblyName("DynamicAssembly");

			// To generate a persistable assembly, specify AssemblyBuilderAccess.RunAndSave.
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);

			// Generate a persistable single-module assembly.
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("myAsm.dll", "myAsm.dll");

			TypeBuilder typeBuilder = modBuilder.DefineType("TreeNode_ReadContext", TypeAttributes.Public | TypeAttributes.Class);

			//TreeNode _target;
			FieldBuilder fb_target = typeBuilder.DefineField("_target", typeof(TreeNode), FieldAttributes.Private);
			//private IByteReader _reader;
			FieldBuilder fb_reader = typeBuilder.DefineField("_reader", typeof(IByteReader), FieldAttributes.Private);
			//private Action<TreeNode> _completed;
			FieldBuilder fb_completed = typeBuilder.DefineField("_completed", typeof(Action<TreeNode>), FieldAttributes.Private);
			//private Action<Exception> _excepted;
			FieldBuilder fb_excepted = typeBuilder.DefineField("_excepted", typeof(Action<Exception>), FieldAttributes.Private);

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

			*/

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
			
			/*
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldfld, fb_reader); //ldfld class EmitTest.IByteReader EmitTest.TreeNode_ReadContext::_reader
			ilGenCb.Emit(OpCodes.Ldc_I4_4);
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldftn, mb1); //ldftn instance void EmitTest.TreeNode_ReadContext::Field3_Readed(uint8[])
			ilGenCb.Emit(OpCodes.Newobj, typeof(Action<byte[]>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) })); //newobj instance void [mscorlib]System.Action`1<uint8[]>::.ctor(object, native int)
			ilGenCb.Emit(OpCodes.Ldarg_0);
			ilGenCb.Emit(OpCodes.Ldfld, fb_excepted);
			ilGenCb.Emit(OpCodes.Callvirt, typeof(IByteReader).GetMethod("Read", new Type[] { typeof(int), typeof(Action<byte[]>), typeof(Action<Exception>) }));
			*/

			ilGenCb.Emit(OpCodes.Ret);
			
			
			
			Type type = typeBuilder.CreateType();
			

			asmBuilder.Save("myAsm.dll");
			return type;
		}
	}
}
