using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public class XdrReaderExample: IReader
	{
		private IByteReader _reader;

		public XdrReaderExample(IByteReader reader)
		{
			_reader = reader;
		}

		public void Read<T>(Action<T> completed, Action<Exception> excepted)
		{
			TypeCacheExample<T>.Read(_reader, completed, excepted);
		}

		public void Read<T>(uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			TypeCacheExample<T>.ReadArray(_reader, len, fix, completed, excepted);
		}
	}
}
