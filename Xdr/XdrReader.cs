using System;

namespace Xdr
{
	internal delegate void ReaderDelegate<T>(IByteReader reader, Action<T> completed, Action<Exception> excepted);

	public static class XdrReader<T>
	{
		private static readonly ReaderDelegate<T> _reader;
		
		
		static XdrReader()
		{
			Type t = typeof(T);

			if (t == typeof(int))
				_reader = (ReaderDelegate<T>)(Delegate)(ReaderDelegate<int>)ReadInt32;

			else
				_reader = Build();
			
		}

		internal static ReaderDelegate<T> Build()
		{
			Type t = typeof(T);

			return null;
		}
		
		public static void Read(IByteReader reader, Action<T> completed, Action<Exception> exceped)
		{
			_reader(reader, completed, exceped);
		}
		
		public static void ReadInt32(IByteReader reader, Action<int> completed, Action<Exception> exceped)
		{
			reader.Read(4, (buff) => completed(XdrEncoding.DecodeInt32(buff)), exceped);
		}
	}
}

