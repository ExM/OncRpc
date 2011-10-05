using System;

namespace Xdr
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
		public static void EncodeInt32(int v, IByteWriter w)
		{
			w.Write((byte)((v >> 0x18) & 0xff));
			w.Write((byte)((v >> 0x10) & 0xff));
			w.Write((byte)((v >> 8) & 0xff));
			w.Write((byte)(v & 0xff));
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
		public static void EncodeUInt32(uint v, IByteWriter w)
		{
			w.Write((byte)((v >> 0x18) & 0xff));
			w.Write((byte)((v >> 0x10) & 0xff));
			w.Write((byte)((v >> 8) & 0xff));
			w.Write((byte)(v & 0xff));
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
		public static void EncodeInt64(long v, IByteWriter w)
		{
			w.Write((byte)((v >> 56) & 0xff));
			w.Write((byte)((v >> 48) & 0xff));
			w.Write((byte)((v >> 40) & 0xff));
			w.Write((byte)((v >> 32) & 0xff));
			w.Write((byte)((v >> 24) & 0xff));
			w.Write((byte)((v >> 16) & 0xff));
			w.Write((byte)((v >>  8) & 0xff));
			w.Write((byte)(v & 0xff));
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
		public static void EncodeUInt64(ulong v, IByteWriter w)
		{
			w.Write((byte)((v >> 56) & 0xff));
			w.Write((byte)((v >> 48) & 0xff));
			w.Write((byte)((v >> 40) & 0xff));
			w.Write((byte)((v >> 32) & 0xff));
			w.Write((byte)((v >> 24) & 0xff));
			w.Write((byte)((v >> 16) & 0xff));
			w.Write((byte)((v >>  8) & 0xff));
			w.Write((byte)(v & 0xff));
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
		public unsafe static void EncodeSingle(Single v, IByteWriter w)
		{
			EncodeInt32(*(int*)(&v), w);
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
		public unsafe static void EncodeDouble(Double v, IByteWriter w)
		{
			EncodeInt64(*(long*)(&v), w);
		}
	}
}

