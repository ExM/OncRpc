using System;

namespace Xdr
{
	public static class XdrReader<T>
	{
		private static readonly Action<IByteReader, Action<T>, Action<Exception>> _read;
		
		
		static XdrReader()
		{
			Type t = typeof(T);

			if(t == typeof(int))
				_read = (Action<IByteReader, Action<T>, Action<Exception>>)(Action<IByteReader, Action<int>, Action<Exception>>)ReadInt32;
			
			else
				_read = null;
			
		}
		
		
		public static void Read(IByteReader reader, Action<T> completed, Action<Exception> exceped)
		{
			_read(reader, completed, exceped);
		}
		
		public static void ReadInt32(IByteReader reader, Action<int> completed, Action<Exception> exceped)
		{
			reader.Read(4, (buff) => completed(XdrEncoding.DecodeInt32(buff)), exceped);
		}
	}
}

