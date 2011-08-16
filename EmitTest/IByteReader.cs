using System;

namespace EmitTest
{
	public interface IByteReader
	{
		void Read(uint count, Action<byte[]> completed, Action<Exception> excepted);
	}
}
