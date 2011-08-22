using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Examples
{
	public static class ReadManyCache<T>
	{
		public static Exception Error;
		public static ReadManyDelegate<T> Instance;

		static ReadManyCache()
		{
			Error = null;
			Instance = null;
			DelegateCache.ReadManyInit(typeof(T));
		}
	}
}
