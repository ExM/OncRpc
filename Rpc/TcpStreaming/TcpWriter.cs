using System;
using System.IO;
using Xdr;
using System.Collections.Generic;

namespace Rpc.TcpStreaming
{
	/// <summary>
	/// generator TCP messages with record mark
	/// http://tools.ietf.org/html/rfc5531#section-11
	/// </summary>
	public class TcpWriter : IByteWriter
	{
		private readonly int _maxBlock;
		private long _pos;
		private byte[] _currentBlock;
		private LinkedList<byte[]> _blocks;

		/// <summary>
		/// generator TCP messages with record mark
		/// </summary>
		/// <param name="maxBlock">maximum block size in the TCP message</param>
		public TcpWriter(int maxBlock)
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
			long offset = 0;
			while(true)
			{
				long len = buffer.LongLength - offset;
			
				if(len <= _maxBlock - _pos)
				{
					Array.Copy(buffer, offset, _currentBlock, _pos, len);
					_pos += len;
					if(_pos >= _maxBlock)
						CreateNextBlock();
				
					return;
				}
			
				Array.Copy(buffer, offset, _currentBlock, _pos, _maxBlock - _pos);
				offset += _maxBlock - _pos;
			
				CreateNextBlock();
			}
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
				CreateNextBlock();
		}
		
		private void CreateNextBlock()
		{
			SetLenth(_currentBlock);
			_blocks.AddLast(_currentBlock);
			_currentBlock = new byte[_maxBlock];
			_pos = 4;
		}
		
		private void SetLastBlock()
		{
			var last = _blocks.Last;
			if(last == null)
				return;

			byte[] block = last.Value;
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

		/// <summary>
		/// create the TCP message (the original object is destroyed)
		/// </summary>
		/// <returns>blocks of TCP message</returns>
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
			
			SetLastBlock();

			LinkedList<byte[]> result = _blocks;
			_currentBlock = null;
			_blocks = null;
			return result;
		}
	}
}

