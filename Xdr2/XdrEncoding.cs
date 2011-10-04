using System;

namespace Xdr2
{
	public static class XdrEncoding
	{
		/// <summary>
		/// Decodes the Int32.
		/// http://tools.ietf.org/html/rfc4506#section-4.1
		/// </summary>
		public static int DecodeInt32(byte[] buff)
		{
			return
				(buff[0] << 0x18) |
				(buff[1] << 0x10) |
				(buff[2]  << 0x08) |
				buff[3];
		}

		/// <summary>
		/// Encodes the Int32.
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
		
		/// <summary>
		/// Decodes the UInt32.
		/// http://tools.ietf.org/html/rfc4506#section-4.2
		/// </summary>
		public static uint DecodeUInt32(byte[] buff)
		{
			return
				((uint)buff[0] << 0x18) |
				((uint)buff[1] << 0x10) |
				((uint)buff[2] << 0x08) |
				(uint)buff[3];
		}
		
		/// <summary>
		/// Encodes the UInt32.
		/// http://tools.ietf.org/html/rfc4506#section-4.2
		/// </summary>
		public static byte[] EncodeUInt32(uint v)
		{
			byte[] b = new byte[4];
			b[0] = (byte)((v >> 0x18) & 0xff);
			b[1] = (byte)((v >> 0x10) & 0xff);
			b[2] = (byte)((v >> 8) & 0xff);
			b[3] = (byte)(v & 0xff);
			return b;
		}

		/// <summary>
		/// Decodes the Int64.
		/// http://tools.ietf.org/html/rfc4506#section-4.5
		/// </summary>
		public static long DecodeInt64(byte[] buff)
		{
			return
				((long)buff[0] << 56) |
				((long)buff[1] << 48) |
				((long)buff[2] << 40) |
				((long)buff[3] << 32) |
				((long)buff[4] << 24) |
				((long)buff[5] << 16) |
				((long)buff[6] << 8) |
				(long)buff[7];
		}

		/// <summary>
		/// Encodes the Int64.
		/// http://tools.ietf.org/html/rfc4506#section-4.5
		/// </summary>
		public static byte[] EncodeInt64(long v)
		{
			byte[] b = new byte[8];
			b[0] = (byte)((v >> 56) & 0xff);
			b[1] = (byte)((v >> 48) & 0xff);
			b[2] = (byte)((v >> 40) & 0xff);
			b[3] = (byte)((v >> 32) & 0xff);
			b[4] = (byte)((v >> 24) & 0xff);
			b[5] = (byte)((v >> 16) & 0xff);
			b[6] = (byte)((v >>  8) & 0xff);
			b[7] = (byte)(v & 0xff);
			return b;
		}

		/// <summary>
		/// Decodes the UInt64.
		/// http://tools.ietf.org/html/rfc4506#section-4.5
		/// </summary>
		public static ulong DecodeUInt64(byte[] buff)
		{
			return
				((ulong)buff[0] << 56) |
				((ulong)buff[1] << 48) |
				((ulong)buff[2] << 40) |
				((ulong)buff[3] << 32) |
				((ulong)buff[4] << 24) |
				((ulong)buff[5] << 16) |
				((ulong)buff[6] << 8) |
				(ulong)buff[7];
		}

		/// <summary>
		/// Encodes the UInt64.
		/// http://tools.ietf.org/html/rfc4506#section-4.5
		/// </summary>
		public static byte[] EncodeUInt64(ulong v)
		{
			byte[] b = new byte[8];
			b[0] = (byte)((v >> 56) & 0xff);
			b[1] = (byte)((v >> 48) & 0xff);
			b[2] = (byte)((v >> 40) & 0xff);
			b[3] = (byte)((v >> 32) & 0xff);
			b[4] = (byte)((v >> 24) & 0xff);
			b[5] = (byte)((v >> 16) & 0xff);
			b[6] = (byte)((v >> 8) & 0xff);
			b[7] = (byte)(v & 0xff);
			return b;
		}

		/// <summary>
		/// Decodes the Single.
		/// http://tools.ietf.org/html/rfc4506#section-4.6
		/// </summary>
		public unsafe static Single DecodeSingle(byte[] buff)
		{
			int num = DecodeInt32(buff);
			return *(float*)(&num);
		}

		/// <summary>
		/// Encodes the Single.
		/// http://tools.ietf.org/html/rfc4506#section-4.6
		/// </summary>
		public unsafe static byte[] EncodeSingle(Single v)
		{
			return EncodeInt32(*(int*)(&v));
		}

		/// <summary>
		/// Decodes the Double.
		/// http://tools.ietf.org/html/rfc4506#section-4.7
		/// </summary>
		public unsafe static Double DecodeDouble(byte[] buff)
		{
			long num = DecodeInt64(buff);
			return *(double*)(&num);
		}

		/// <summary>
		/// Encodes the Double.
		/// http://tools.ietf.org/html/rfc4506#section-4.7
		/// </summary>
		public unsafe static byte[] EncodeDouble(Double v)
		{
			return EncodeInt64(*(long*)(&v));
		}
	}
}

