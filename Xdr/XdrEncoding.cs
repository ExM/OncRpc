using System;

namespace Xdr
{
	public static class XdrEncoding
	{
		/// <summary>
		/// Decodes the int32.
		/// http://tools.ietf.org/html/rfc4506#section-4.1
		/// </summary>
		public static int DecodeInt32(byte[] buff)
		{
			return (buff[0] << 0x18) | (buff[1] << 0x10) | (buff[2]  << 0x08) | buff[3];
		}
		/// <summary>
		/// Encodes the int32.
		/// http://tools.ietf.org/html/rfc4506#section-4.1
		/// </summary>
		public static byte[] EncodeInt32(int v)
		{
			byte[] b = new byte[4];
			b[0] = (byte)((v >> 0x18) & 0xff);
			b[1] = (byte)((v >> 0x10) & 0xff);
			b[2] = (byte)((v >> 8) & 0xff);
			b[3] = (byte)(v & 0xff);
			return b;
		}
		
	}
}

