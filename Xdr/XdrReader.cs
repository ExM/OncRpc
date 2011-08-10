using System;

namespace Xdr
{
	public static class XdrReader<T>
	{
		private static readonly ReaderDelegate<T> _reader;
		
		static XdrReader()
		{
			_reader = XdrReadBuilder.Build<T>();
		}

		public static void Read(IByteReader reader, Action<T> completed, Action<Exception> excepted)
		{
			_reader(reader, completed, excepted);
		}

		public static void Read(IByteReader reader, uint length, bool allowLess, Action<T[]> completed, Action<Exception> excepted)
		{
			



		}
		

	}
}

