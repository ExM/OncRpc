using System;

namespace Xdr
{
	internal delegate void ReaderDelegate<T>(IByteReader reader, Action<T> completed, Action<Exception> excepted);

	internal static class TypeCache<T>
	{
		internal static Exception ReadContextExcepted = null;
		internal static Type ReadContextType = null;
		
		internal static void Read(IByteReader reader, Action<T> completed, Action<Exception> excepted)
		{
			Activator.CreateInstance(ReadContextType, reader, completed, excepted);
		}
	}
}

