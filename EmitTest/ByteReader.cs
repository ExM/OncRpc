using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public class ByteReader: IByteReader
	{
		public void Read(uint count, Action<byte[]> completed, Action<Exception> excepted)
		{
			completed(new byte[] { 0x00, 0x00, 0x00, 0x05});
		}


		public void Throw(Exception ex, Action<Exception> excepted)
		{
			excepted(ex);
		}
	}
}
