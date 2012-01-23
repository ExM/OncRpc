using System;
using System.IO;
using Xdr;

namespace Rpc.UdpDatagrams
{
	/// <summary>
	/// parser of RPC message
	/// </summary>
	public class UdpReader: IByteReader
	{
		private int _pos = 0;
		private int _leftToRead;
		private byte[] _bytes = null;

		/// <summary>
		/// parser of RPC message
		/// </summary>
		/// <param name="bytes">raw</param>
		public UdpReader(byte[] bytes)
		{
			_pos = 0;
			_leftToRead = bytes.Length;
			_bytes = bytes;
		}

		/// <summary>
		/// read an array of length 'count' bytes
		/// </summary>
		/// <param name="count">required bytes</param>
		/// <returns></returns>
		public byte[] Read(uint count)
		{
			if(_leftToRead < count)
				throw UnexpectedEnd();
			
			int icount = (int)count;
			
			byte[] result = new byte[count];
			Array.Copy(_bytes, _pos, result, 0, count);
			_pos += icount;
			_leftToRead -= icount;
			return result;
		}

		/// <summary>
		/// read one byte
		/// </summary>
		/// <returns></returns>
		public byte Read()
		{
			if(_leftToRead < 1)
				throw UnexpectedEnd();

			byte result = _bytes[_pos];
			_pos++;
			_leftToRead--;
			return result;
		}
		
		private static Exception UnexpectedEnd()
		{
			return new RpcException("unexpected end of RPC message");
		}

		/// <summary>
		/// check the completeness of parsing
		/// </summary>
		public void CheckEmpty()
		{
			if(_leftToRead > 0)
				throw new RpcException("RPC message parsed not completely");
		}
	}
}

