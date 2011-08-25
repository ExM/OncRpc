using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Xdr.Translating;

namespace Xdr.Examples
{
	public static class ReadOneCache<T>
	{
		public static readonly ReadOneDelegate<T> Instance;
		
		static ReadOneCache()
		{
			DelegateCache.BuildRequest(typeof(T), MethodType.ReadOne);
		}
	}
}
