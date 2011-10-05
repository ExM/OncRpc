using System;

namespace Xdr2
{
	public interface IByteReader
	{
		byte[] Read(uint count);
		byte Read();
	}
}

