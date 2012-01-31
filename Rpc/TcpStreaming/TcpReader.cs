using System;
using System.Collections.Generic;
using Xdr;

namespace Rpc.TcpStreaming
{
	/// <summary>
	/// parser of RPC message received from TCP protocol
	/// </summary>
	public class TcpReader : IByteReader, IMsgReader
	{
		private LinkedList<byte[]> _blocks = new LinkedList<byte[]>();
		private long _pos = 0;
		private byte[] _currentBlock = null;
		private long _currentBlockSize = 0;

		/// <summary>
		/// parser of RPC message received from TCP protocol
		/// </summary>
		public TcpReader()
		{
		}

		/// <summary>
		/// append the block of RPC message received from TCP protocol
		/// </summary>
		public void AppendBlock(byte[] block)
		{
			if (_currentBlock == null)
			{
				_currentBlock = block;
				_currentBlockSize = _currentBlock.LongLength;
			}
			else
				_blocks.AddLast(block);
		}
		
		private void NextBlock()
		{
			_pos = 0;
			if (_blocks.First == null)
			{
				_currentBlock = null;
				_currentBlockSize = 0;
			}
			else
			{
				_currentBlock = _blocks.First.Value;
				_currentBlockSize = _currentBlock.LongLength;
				_blocks.RemoveFirst();
			}
		}

		/// <summary>
		/// read an array of length 'count' bytes
		/// </summary>
		/// <param name="count">required bytes</param>
		/// <returns></returns>
		public byte[] Read(uint count)
		{
			byte[] buffer = new byte[count];
			long offset = 0;
			
			while(true)
			{
				if (_currentBlock == null)
					throw UnexpectedEnd();

				long len = count - offset;

				if (len <= _currentBlockSize - _pos)
				{
					Array.Copy(_currentBlock, _pos, buffer, offset, len);
					_pos += len;
					if (_pos >= _currentBlockSize)
						NextBlock();
				
					return buffer;
				}

				Array.Copy(_currentBlock, _pos, buffer, offset, _currentBlockSize - _pos);
				offset += _currentBlockSize - _pos;
			
				NextBlock();
			}
		}

		/// <summary>
		/// read one byte
		/// </summary>
		public byte Read()
		{
			if (_currentBlock == null)
				throw UnexpectedEnd();

			byte result = _currentBlock[_pos];
			_pos++;

			if(_pos >= _currentBlockSize)
				NextBlock();
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
			if(_currentBlock != null)
				throw new RpcException("RPC message parsed not completely");
		}
	}
}

