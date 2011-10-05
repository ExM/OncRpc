using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Xdr2.Reading.Emit
{
	public class OneCacheDescription: GenCacheDescription
	{
		public OneCacheDescription(ModuleBuilder modBuilder, DelegateCacheDescription delegCacheDesc)
			: base(modBuilder, delegCacheDesc, "OneCache", OpaqueType.One)
		{
		}
	}

}
