using Xdr.Translating;

namespace Xdr.StaticSingletones
{
	public static class ReadOneCache<T>
	{
		public static ReadOneDelegate<T> Instance;
		
		static ReadOneCache()
		{
			DelegateCache.BuildRequest(typeof(T), MethodType.ReadOne);
		}
	}
}
