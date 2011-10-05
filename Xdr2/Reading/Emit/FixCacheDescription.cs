using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Xdr2.Reading.Emit
{
	public class FixCacheDescription: GenCacheDescription
	{
		public FixCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "FixCache", OpaqueType.Fix)
		{
		}
	}
}
