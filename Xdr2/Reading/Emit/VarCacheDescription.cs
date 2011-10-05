using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Xdr2.Reading.Emit
{
	public class VarCacheDescription: GenCacheDescription
	{
		public VarCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "VarCache", OpaqueType.Var)
		{
		}
	}
}
