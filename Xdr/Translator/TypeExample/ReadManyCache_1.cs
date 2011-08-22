using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Examples
{
	public static class ReadManyCache<T>
	{
		public static readonly ReadManyDelegate<T> Instance;
		public static readonly Exception Error;
		
		static ReadManyCache()
		{
			try
			{
				Instance = (ReadManyDelegate<T>)DelegateCache.ReadManyBuild(typeof(T));
			}
			catch(Exception ex)
			{
				Error = ex;
				Instance = ErrorStub;
			}
		}
		
		private static void ErrorStub(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			reader.Throw(Error, excepted);
		}
	}
}
