using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr2;

namespace EmitTest
{
	public static class DelegateCache
	{
		public static Action<Type, OpaqueType> BuildRequest;
	}
	
	public static class OneCache<T>
	{
		public static ReadOneDelegate<T> Instance;
		
		static OneCache()
		{
			DelegateCache.BuildRequest(typeof(T), OpaqueType.One);
		}
	}
}
