using System;
using System.IO;

namespace Xdr
{
	public static class XdrStreamExtension
	{
		
		public static void XdrRead<T>(this Stream stream, Action<T> completed, Action<Exception> exceped)
		{
			
			
			
		}
		
		public static void XdrWrite<T>(this Stream stream, T val, Action completed, Action<Exception> exceped)
		{
			
			
			
		}
	}
}

