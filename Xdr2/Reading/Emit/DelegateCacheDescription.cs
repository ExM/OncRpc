using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Xdr2.Reading.Emit
{
	public class DelegateCacheDescription
	{
		public readonly Type Result;
		public readonly FieldInfo BuildRequest;

		public DelegateCacheDescription(ModuleBuilder modBuilder)
		{
			TypeBuilder typeBuilder = modBuilder.DefineType("DelegateCache",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);

			FieldBuilder fb_BuildRequest = typeBuilder.DefineField("BuildRequest", typeof(Action<Type, OpaqueType>), FieldAttributes.Public | FieldAttributes.Static);

			Result = typeBuilder.CreateType();
			BuildRequest = fb_BuildRequest;
		}
	}
}
