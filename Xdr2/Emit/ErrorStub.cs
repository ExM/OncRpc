using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Xdr2
{
	internal static class ErrorStub
	{
		private static Delegate StubDelegate(Exception ex, string method, Type targetType, Type genDelegateType)
		{
			Type stubType = typeof(ErrorStub<>).MakeGenericType(targetType);
			object stubInstance = Activator.CreateInstance(stubType, ex);
			MethodInfo mi = stubType.GetMethod(method);
			return Delegate.CreateDelegate(genDelegateType.MakeGenericType(targetType), stubInstance, mi);
		}
		
		internal static Delegate ReadOneDelegate(Type t, Exception ex)
		{
			return StubDelegate(ex, "ReadOne", t, typeof(ReadOneDelegate<>));
		}
		
		internal static Delegate ReadManyDelegate(Type t, Exception ex)
		{
			return StubDelegate(ex, "ReadMany", t, typeof(ReadManyDelegate<>));
		}
		
		internal static Delegate WriteOneDelegate(Type t, Exception ex)
		{
			return StubDelegate(ex, "WriteOne", t, typeof(WriteOneDelegate<>));
		}
		
		internal static Delegate WriteManyDelegate(Type t, Exception ex)
		{
			return StubDelegate(ex, "WriteMany", t, typeof(WriteManyDelegate<>));
		}
	}
}
