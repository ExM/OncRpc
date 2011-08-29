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
	public class ReadOneCacheDescription: GenCacheDescription
	{
		public ReadOneCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			:base(modBuilder, delegCacheDesc, "ReadOneCache", MethodType.ReadOne)
		{
		}
	}

}
