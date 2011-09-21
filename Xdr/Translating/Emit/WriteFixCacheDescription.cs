using System;
using System.Reflection;
using System.Reflection.Emit;
using Xdr.Translating;

namespace Xdr.Translating.Emit
{
	public class WriteFixCacheDescription: GenCacheDescription
	{
		public WriteFixCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "WriteFixCache", MethodType.WriteFix)
		{
		}
	}
}
