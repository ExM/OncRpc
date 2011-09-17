using System;
using System.Reflection;
using System.Reflection.Emit;
using Xdr.Translating;

namespace Xdr.Translating.Emit
{
	public class ReadVarCacheDescription: GenCacheDescription
	{
		public ReadVarCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "ReadVarCache", MethodType.ReadVar)
		{
		}
	}
}
