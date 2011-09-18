using System;
using System.Reflection;
using System.Reflection.Emit;
using Xdr.Translating;

namespace Xdr.Translating.Emit
{
	public class WriteVarCacheDescription: GenCacheDescription
	{
		public WriteVarCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "WriteVarCache", MethodType.WriteVar)
		{
		}
	}
}
