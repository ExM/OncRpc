using System;
using System.IO;

namespace Xdr
{
	public static class TestExtensions
	{
		public static void Write(this Stream stream, params byte[] buf)
		{
			stream.Write(buf, 0, buf.Length);
		}
	}
}

