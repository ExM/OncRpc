using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Examples
{
	public static class ReadManyCache<T>
	{
		public static readonly ReadManyDelegate<T> Instance;
		
		static ReadManyCache()
		{
			DelegateCache.BuildRequest(typeof(T), MethodType.ReadMany);
		}
	}
}
