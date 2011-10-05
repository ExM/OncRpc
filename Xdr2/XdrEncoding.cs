using System;

namespace Xdr2
{
	public static class XdrEncoding
	{
		/// <summary>
		/// Decodes the Int32.
		/// http://tools.ietf.org/html/rfc4506#section-4.1
		/// </summary>
		public static int DecodeInt32(IByteReader r)
		{
			return
				(r.Read() << 0x18) |
				(r.Read() << 0x10) |
				(r.Read() << 0x08) |
				r.Read();
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
		public static uint DecodeUInt32(IByteReader r)
		{
			return
				((uint)r.Read() << 0x18) |
				((uint)r.Read() << 0x10) |
				((uint)r.Read() << 0x08) |
				(uint)r.Read();
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
		public static long DecodeInt64(IByteReader r)
		{
			return
				((long)r.Read() << 56) |
				((long)r.Read() << 48) |
				((long)r.Read() << 40) |
				((long)r.Read() << 32) |
				((long)r.Read() << 24) |
				((long)r.Read() << 16) |
				((long)r.Read() << 8) |
				(long)r.Read();
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
		public static ulong DecodeUInt64(IByteReader r)
		{
			return
				((ulong)r.Read() << 56) |
				((ulong)r.Read() << 48) |
				((ulong)r.Read() << 40) |
				((ulong)r.Read() << 32) |
				((ulong)r.Read() << 24) |
				((ulong)r.Read() << 16) |
				((ulong)r.Read() << 8) |
				(ulong)r.Read();
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
		public unsafe static Single DecodeSingle(IByteReader r)
		{
			int num = DecodeInt32(r);
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
		public unsafe static Double DecodeDouble(IByteReader r)
		{
			long num = DecodeInt64(r);
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

