using Xdr.Translating;

namespace Xdr.StaticSingletones
{
	public static class WriteManyCache<T>
	{
		public static WriteManyDelegate<T> Instance;

		static WriteManyCache()
		{
			DelegateCache.BuildRequest(typeof(T), MethodType.WriteMany);
		}
	}
}
