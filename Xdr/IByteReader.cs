using System;

namespace Xdr
{
	public interface IByteReader
	{
		void Read(uint count, Action<byte[]> completed, Action<Exception> excepted);
	}
}

