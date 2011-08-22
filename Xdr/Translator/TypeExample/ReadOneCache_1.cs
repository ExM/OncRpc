using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Examples
{
	public static class ReadOneCache<T>
	{
		public static readonly ReadOneDelegate<T> Instance;
		public static readonly Exception Error;
		
		static ReadOneCache()
		{
			try
			{
				Instance = (ReadOneDelegate<T>)DelegateCache.ReadOneBuild(typeof(T));
			}
			catch(Exception ex)
			{
				Error = ex;
				Instance = ErrorStub;
			}
		}
		
		private static void ErrorStub(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			reader.Throw(Error, excepted);
		}
	}
}
