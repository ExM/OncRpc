using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Examples
{
	public static class ReadOneCache<T>
	{
		public static Exception Error;
		public static ReadOneDelegate<T> Instance;

		static ReadOneCache()
		{
			Error = null;
			Instance = null;
			DelegateCache.ReadOneInit(typeof(T));
		}
	}
}
