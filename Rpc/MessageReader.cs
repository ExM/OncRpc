using System;
using System.IO;
using Xdr;

namespace Rpc
{
	/// <summary>
	/// parser of RPC message
	/// </summary>
	public class MessageReader: IByteReader
	{
		private uint _pos = 0;
		private byte[] _bytes;

		/// <summary>
		/// parser of RPC message
		/// </summary>
		/// <param name="bytes">raw</param>
		public MessageReader(params byte[] bytes)
		{
			_bytes = bytes;
		}

		/// <summary>
		/// current position in byte array
		/// </summary>
		public uint Position
		{
			get
			{
				return _pos;
			}
		}

		/// <summary>
		/// read an array of length 'count' bytes
		/// </summary>
		/// <param name="count">required bytes</param>
		/// <returns></returns>
		public byte[] Read(uint count)
		{
			CheckExist(count);
			byte[] result = new byte[count];
			Array.Copy(_bytes, _pos, result, 0, count);
			_pos += count;
			return result;
		}

		/// <summary>
		/// read one byte
		/// </summary>
		/// <returns></returns>
		public byte Read()
		{
			CheckExist(1);
			byte result = _bytes[_pos];
			_pos++;
			return result;
		}

		private void CheckExist(uint p)
		{
			if (_pos + p > _bytes.LongLength)
				throw new RpcException("unexpected end of RPC message");
		}

		/// <summary>
		/// check the completeness of parsing
		/// </summary>
		public void CheckEmpty()
		{
			if (_pos < _bytes.LongLength)
				throw new RpcException("RPC message parsed not completely");
		}
	}
}

