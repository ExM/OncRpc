using System;
using System.Reflection;
using System.Reflection.Emit;
using Xdr.Translating;

namespace Xdr.Translating.Emit
{
	public class ReadFixCacheDescription: GenCacheDescription
	{
		public ReadFixCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "ReadFixCache", MethodType.ReadFix)
		{
		}
	}
}
