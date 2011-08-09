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

		public static void Read(IByteReader reader, Action<T> completed, Action<Exception> exceped)
		{
			_reader(reader, completed, exceped);
		}
		
		/*
		public static void ReadInt32(IByteReader reader, Action<int> completed, Action<Exception> exceped)
		{
			reader.Read(4, (buff) => completed(XdrEncoding.DecodeInt32(buff)), exceped);
		}
		*/
	}
}

