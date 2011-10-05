using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Xdr.Emit
{
	public class BuildBinderDescription
	{
		public readonly Type Result;
		public readonly FieldInfo BuildRequest;

		public BuildBinderDescription(ModuleBuilder modBuilder)
		{
			TypeBuilder typeBuilder = modBuilder.DefineType("BuildBinder",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);

			FieldBuilder fb_request = typeBuilder.DefineField("Request", typeof(Action<Type, OpaqueType>), FieldAttributes.Public | FieldAttributes.Static);

			Result = typeBuilder.CreateType();
			BuildRequest = fb_request;
		}
	}
}
