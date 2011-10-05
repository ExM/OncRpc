using System;

namespace Xdr
{
	public interface IByteReader
	{
		byte[] Read(uint count);
		byte Read();
	}
}

