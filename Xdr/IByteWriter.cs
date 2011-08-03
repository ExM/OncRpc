using System;

namespace Xdr
{
	public interface IByteWriter
	{
		void Write(byte[] buffer, Action completed, Action<Exception> excepted);
	}
}

