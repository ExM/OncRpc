using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Rpc.MessageProtocol;
using Xdr;
using Rpc.BindingProtocols;
using System.Linq;
using System.Threading;
using Rpc.TcpStreaming;

namespace Rpc
{
	[TestFixture]
	public class TcpMessageTest
	{
		[Test]
		public void BuildMessageOneByte()
		{
			byte[] sourceArray = @"
F1E2D3C4 00000000 00
000002 000186A0 0000
0002 00000004 000000
00 00000000 00000000
00000000".LogToArray();
			
			TcpMessage msg = new TcpMessage(13);
			
			foreach(var b in sourceArray)
				msg.Write(b);
			
			List<byte[]> blocks = new List<byte[]>(msg.Build());
			Assert.AreEqual(5, blocks.Count);
			Assert.AreEqual("00000009 F1E2D3C4 00000000 00".LogToArray(), blocks[0]);
			Assert.AreEqual("00000009 000002 000186A0 0000".LogToArray(), blocks[1]);
			Assert.AreEqual("00000009 0002 00000004 000000".LogToArray(), blocks[2]);
			Assert.AreEqual("00000009 00 00000000 00000000".LogToArray(), blocks[3]);
			Assert.AreEqual("80000004 00000000".LogToArray(), blocks[4]);
		}
	}
}

