using System;

namespace Xdr
{
	public static class ByteReaderExtensions
	{
		public static void Read<T>(this IByteReader reader, Action<T> completed, Action<Exception> excepted)
		{
			XdrReader<T>.Read(reader, completed, excepted);
		}
	}
}

