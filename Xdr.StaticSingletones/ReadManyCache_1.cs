using Xdr.Translating;

namespace Xdr.StaticSingletones
{
	public static class ReadManyCache<T>
	{
		public static ReadManyDelegate<T> Instance;
		
		static ReadManyCache()
		{
			DelegateCache.BuildRequest(typeof(T), MethodType.ReadMany);
		}
	}
}
