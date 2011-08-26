using Xdr.Translating;

namespace Xdr.StaticSingletones
{
	public static class WriteOneCache<T>
	{
		public static WriteOneDelegate<T> Instance;

		static WriteOneCache()
		{
			DelegateCache.BuildRequest(typeof(T), MethodType.WriteOne);
		}
	}
}
