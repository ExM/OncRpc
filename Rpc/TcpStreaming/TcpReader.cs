using System;
using System.IO;
using Xdr;
using System.Collections.Generic;

namespace Rpc.TcpStreaming
{

	public class TcpReader : IByteReader
	{
		private LinkedList<byte[]> _blocks = new LinkedList<byte[]>();
		
		public TcpReader()
		{
		}
		
		public void AppendBlock(byte[] block)
		{
			_blocks.AddLast(block);
		}
		
		public byte[] Read(uint count)
		{
			//TODO
			throw new NotImplementedException();
		}

		public byte Read()
		{
			//TODO
			throw new NotImplementedException();
		}
	}
}

