using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public static class DelegateCache
	{
		public static Action<Type, OpaqueType> BuildRequest;
	}
	
	public static class OneCache<T>
	{
		public static volatile ReadOneDelegate<T> Instance;
		
		static OneCache()
		{
			DelegateCache.BuildRequest(typeof(T), OpaqueType.One);
		}
	}
}
