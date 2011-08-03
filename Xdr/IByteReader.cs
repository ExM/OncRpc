using System;

namespace Xdr
{
	public interface IByteReader
	{
		void Read(int count, Action<byte[]> completed, Action<Exception> excepted);
	}
}

