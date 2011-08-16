using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Xdr.Emit
{
	public interface IXdrFieldDesc
	{
		MethodBuilder CreateRead(TypeBuilder typeBuilder, FieldBuilder targetField, out ILGenerator il);
		void AppendCall(ILGenerator il, FieldBuilder readerField, MethodBuilder nextMethod, FieldBuilder exceptedField, Func<Type, Delegate> dependencyResolver);
	}
}

