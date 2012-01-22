using System;
using System.IO;
using Xdr;
using System.Collections.Generic;

namespace Rpc.TcpStreaming
{

	public class TcpReader : IByteReader
	{
		private LinkedList<byte[]> _blocks = new LinkedList<byte[]>();
		private int _pos = 0;
		private byte[] _currentBlock = null;
		
		public TcpReader()
		{
		}
		
		public void AppendBlock(byte[] block)
		{
			_blocks.AddLast(block);
		}
		
		public void PrepareRead()
		{
			if(_blocks.First != null)
				_currentBlock = _blocks.First.Value;
		}
		
		private void NextBlock()
		{
			_blocks.RemoveFirst();
			_pos = 0;
			if(_blocks.First == null)
				_currentBlock = null;
			else
				_currentBlock = _blocks.First.Value;
		}
		
		public byte[] Read(uint count)
		{
			byte[] buffer = new byte[count];
			int offset = 0;
			
			while(true)
			{
				int len = buffer.Length - offset;
			
				if(len <= _currentBlock.Length - _pos)
				{
					Array.Copy(_currentBlock, _pos, buffer, offset, len);
					_pos += len;
					if(_pos >= _currentBlock.Length)
						NextBlock();
				
					return buffer;
				}
			
				Array.Copy(_currentBlock, _pos, buffer, offset, _currentBlock.Length - _pos);
				offset += _currentBlock.Length - _pos;
			
				NextBlock();
			}
		}

		public byte Read()
		{
			byte result = _currentBlock[_pos];
			_pos++;
			
			if(_pos >= _currentBlock.Length)
				NextBlock();
			return result;
		}
	}
}

