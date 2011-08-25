using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Xdr.Translating;
using System.Reflection.Emit;
using System.Reflection;

namespace Xdr.Translating.Emit
{
	public class DelegateCacheDescription
	{
		public readonly Type Type;
		public readonly FieldInfo BuildRequest;

		public DelegateCacheDescription(ModuleBuilder modBuilder)
		{
			TypeBuilder typeBuilder = modBuilder.DefineType("DelegateCache",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
			//public static Action<Type, MethodType> BuildRequest = null;
			FieldBuilder fb_BuildRequest = typeBuilder.DefineField("BuildRequest", typeof(Action<Type, MethodType>), FieldAttributes.Public | FieldAttributes.Static);

			Type = typeBuilder.CreateType();
			BuildRequest = fb_BuildRequest;
		}
	}
}
