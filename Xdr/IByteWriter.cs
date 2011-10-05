using System;

namespace Xdr
{
	public interface IByteWriter
	{
		void Write(byte[] buffer);
		void Write(byte b);
	}
}

