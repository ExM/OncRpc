using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public static class TypeCacheExample<T>
	{
		public static Action<IByteReader, Action<T>, Action<Exception>> Read;
		public static Action<IByteReader, uint, bool, Action<T>, Action<Exception>> ReadArray;

		static TypeCacheExample()
		{
			Read = ReadStub;
			ReadArray = ReadArrayStub;
		}
		/*
	IL_0000: ldnull
	IL_0001: ldftn void class EmitTest.TypeCacheExample`1<!T>::ReadStub(class [Xdr]Xdr.IByteReader, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>)
	IL_0007: newobj instance void class [mscorlib]System.Action`3<class [Xdr]Xdr.IByteReader, class [mscorlib]System.Action`1<!T>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>>::.ctor(object, native int)
	IL_000c: stsfld class [mscorlib]System.Action`3<class [Xdr]Xdr.IByteReader, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>> class EmitTest.TypeCacheExample`1<!T>::Read
	IL_0011: ldnull
	IL_0012: ldftn void class EmitTest.TypeCacheExample`1<!T>::ReadArrayStub(class [Xdr]Xdr.IByteReader, uint32, bool, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>)
	IL_0018: newobj instance void class [mscorlib]System.Action`5<class [Xdr]Xdr.IByteReader, uint32, bool, class [mscorlib]System.Action`1<!T>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>>::.ctor(object, native int)
	IL_001d: stsfld class [mscorlib]System.Action`5<class [Xdr]Xdr.IByteReader, uint32, bool, class [mscorlib]System.Action`1<!0>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>> class EmitTest.TypeCacheExample`1<!T>::ReadArray
	IL_0022: ret
		*/

		private static void ReadStub(IByteReader reader, Action<T> completed, Action<Exception> excepted)
		{
			excepted(UnknownType());
		}

		/*
	IL_0000: ldarg.2
	IL_0001: call class [mscorlib]System.Exception class EmitTest.TypeCacheExample`1<!T>::UnknownType()
	IL_0006: callvirt instance void class [mscorlib]System.Action`1<class [mscorlib]System.Exception>::Invoke(!0)
	IL_000b: ret
		*/
		
		private static void ReadArrayStub(IByteReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			excepted(UnknownType());
		}
		/*
	IL_0000: ldarg.s excepted
	IL_0002: call class [mscorlib]System.Exception class EmitTest.TypeCacheExample`1<!T>::UnknownType()
	IL_0007: callvirt instance void class [mscorlib]System.Action`1<class [mscorlib]System.Exception>::Invoke(!0)
	IL_000c: ret
		*/
		private static Exception UnknownType()
		{
			string message = string.Format("unknown type `{0}'", typeof(T).FullName);
			return new NotImplementedException(message);
		}
		/*
.method private hidebysig static 
	class [mscorlib]System.Exception UnknownType () cil managed 
{
	// Method begins at RVA 0x2550
	// Code size 33 (0x21)
	.maxstack 2
	.locals init (
		[0] string message
	)

	IL_0000: ldstr "unknown type `{0}'"
	IL_0005: ldtoken !T
	IL_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
	IL_000f: callvirt instance string [mscorlib]System.Type::get_FullName()
	IL_0014: call string [mscorlib]System.String::Format(string, object)
	IL_0019: stloc.0
	IL_001a: ldloc.0
	IL_001b: newobj instance void [mscorlib]System.NotImplementedException::.ctor(string)
	IL_0020: ret
} // end of method TypeCacheExample`1::UnknownType
		 * 
		 */
	}
}
