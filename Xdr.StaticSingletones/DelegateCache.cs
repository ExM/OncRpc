using System;
using Xdr.Translating;

namespace Xdr.StaticSingletones
{
	public static class DelegateCache
	{
		public static Action<Type, MethodType> BuildRequest = null;
	}
}
