using System;
using System.IO;
using Xdr;
using System.Collections.Generic;

namespace Rpc.TcpStreaming
{
	
	/// <summary>
	/// 
	/// http://tools.ietf.org/html/rfc5531#section-11
	/// </summary>
	public class TcpMessage : IByteWriter
	{
		private readonly int _maxBlock;
		private int _pos;
		private byte[] _currentBlock;
		private LinkedList<byte[]> _blocks;

		/// <summary>
		/// 
		/// </summary>
		public TcpMessage(int maxBlock)
		{
			_maxBlock = maxBlock;
			_pos = 4;
			_currentBlock = new byte[_maxBlock];
			_blocks = new LinkedList<byte[]>();
		}

		/// <summary>
		/// write array of bytes
		/// </summary>
		/// <param name="buffer"></param>
		public void Write(byte[] buffer)
		{
			
		}

		/// <summary>
		/// write byte
		/// </summary>
		/// <param name="b"></param>
		public void Write(byte b)
		{
			_currentBlock[_pos] = b;
			_pos++;
			
			if(_pos >= _maxBlock)
			{
				SetLenth(_currentBlock);
				_blocks.AddLast(_currentBlock);
				_currentBlock = new byte[_maxBlock];
				_pos = 4;
			}
		}
		
		private void SetLastBlock(byte[] block)
		{
			block[0] = (byte)(block[0] | 0x80);
		}
		
		private void SetLenth(byte[] block)
		{
			int len = block.Length - 4;
			block[0] = (byte)((len >> 0x18) & 0xff);
			block[1] = (byte)((len >> 0x10) & 0xff);
			block[2] = (byte)((len >> 8) & 0xff);
			block[3] = (byte)(len & 0xff);
		}

		public LinkedList<byte[]> Build()
		{
			if(_pos != 4) // _currentBlock is not empty
			{
				byte[] shortBlock = new byte[_pos];
				Array.Copy(_currentBlock, shortBlock, _pos);
				SetLenth(shortBlock);
				_blocks.AddLast(shortBlock);
				_pos = 4;
			}
			
			SetLastBlock(_blocks.Last.Value);

			LinkedList<byte[]> result = _blocks;
			_blocks = new LinkedList<byte[]>();
			return result;
		}
	}
}

