using System;
using System.Reflection;
using System.Reflection.Emit;
using Xdr.Translating;

namespace Xdr.Translating.Emit
{
	public class WriteManyCacheDescription: GenCacheDescription
	{
		public WriteManyCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "WriteManyCache", MethodType.WriteMany)
		{
		}
	}
}
