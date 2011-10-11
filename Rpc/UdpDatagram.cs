using System;
using System.IO;
using Xdr;

namespace Rpc
{
	/// <summary>
	/// generator of UDP datagram
	/// </summary>
	public class UdpDatagram : IByteWriter
	{
		private const int _max = 65535;
		private MemoryStream _stream = new MemoryStream();

		/// <summary>
		/// generator of UDP datagram
		/// </summary>
		public UdpDatagram()
		{
		}

		/// <summary>
		/// write array of bytes
		/// </summary>
		/// <param name="buffer"></param>
		public void Write(byte[] buffer)
		{
			_stream.Write(buffer, 0, buffer.Length);
			if (_stream.Position > _max)
				throw new RpcException("UDP datagram size is exceeded");
		}

		/// <summary>
		/// write byte
		/// </summary>
		/// <param name="b"></param>
		public void Write(byte b)
		{
			_stream.WriteByte(b);
			if (_stream.Position > _max)
				throw new RpcException("UDP datagram size is exceeded");
		}

		/// <summary>
		/// convert to byte array
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray()
		{
			_stream.Position = 0;
			return _stream.ToArray();
		}
	}
}

