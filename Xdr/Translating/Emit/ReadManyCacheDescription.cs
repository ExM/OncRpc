using System;
using System.Reflection;
using System.Reflection.Emit;
using Xdr.Translating;

namespace Xdr.Translating.Emit
{
	public class ReadManyCacheDescription: GenCacheDescription
	{
		public ReadManyCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "ReadManyCache", MethodType.ReadMany)
		{
		}
	}
}
