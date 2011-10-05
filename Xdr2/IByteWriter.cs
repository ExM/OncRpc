using System;

namespace Xdr2
{
	public interface IByteWriter
	{
		void Write(byte[] buffer);
		void Write(byte b);
	}
}

