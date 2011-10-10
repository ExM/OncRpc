using System;
using System.IO;
using System.Collections.Generic;
using Xdr;

namespace Rpc
{
	public class ByteWriter: IByteWriter
	{
		private List<byte> _bytes;

		public ByteWriter()
		{
			_bytes = new List<byte>();
		}
		
		public byte[] ToArray()
		{
			return _bytes.ToArray();
		}

		public void Write(byte[] buffer)
		{
			_bytes.AddRange(buffer);
		}

		public void Write(byte b)
		{
			_bytes.Add(b);
		}
	}
}

