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

namespace Rpc.TcpStreaming
{
	[TestFixture]
	public class TcpWriterTest
	{
		[Test]
		public void OneByte()
		{
			byte[] sourceArray = @"
F1E2D3C4 00000000 00
000002 000186A0 0000
0002 00000004 000000
00 00000000 00000000
00000000".LogToArray();
			
			TcpWriter msg = new TcpWriter(13);
			
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
		
		[Test]
		public void Short1()
		{
			TcpWriter msg = new TcpWriter(13);
			
			msg.Write("F1E2D3C4 00000000".LogToArray());
			
			List<byte[]> blocks = new List<byte[]>(msg.Build());
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("80000008 F1E2D3C4 00000000".LogToArray(), blocks[0]);
		}
		
		[Test]
		public void Short2()
		{
			TcpWriter msg = new TcpWriter(13);
			
			msg.Write("F1E2D3C4 00000000 00".LogToArray());
			
			List<byte[]> blocks = new List<byte[]>(msg.Build());
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("80000009 F1E2D3C4 00000000 00".LogToArray(), blocks[0]);
		}
		
		[Test]
		public void TwoBlocks()
		{
			TcpWriter msg = new TcpWriter(13);
			
			msg.Write("F1E2D3C4 00000000 00FF0000".LogToArray());
			
			List<byte[]> blocks = new List<byte[]>(msg.Build());
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("00000009 F1E2D3C4 00000000 00".LogToArray(), blocks[0]);
			Assert.AreEqual("80000003 FF0000".LogToArray(), blocks[1]);
		}
		
		[Test]
		public void AllBytes()
		{
			TcpWriter msg = new TcpWriter(13);
			
			msg.Write("F1E2D3C4 00000000 00000002 000186A0 00000002 00000004 00000000 00000000 00000000 00000000".LogToArray());
			
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

