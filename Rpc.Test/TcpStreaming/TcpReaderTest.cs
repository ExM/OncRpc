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
	public class TcpReaderTest
	{
		[Test]
		public void MatchBounds()
		{
			TcpReader r = new TcpReader();
			
			r.AppendBlock("F1E2D3C4 00000000 00".LogToArray());
			r.AppendBlock("000002 000186A0 0000".LogToArray());
			r.AppendBlock("0002 00000004".LogToArray());
			r.PrepareRead();
			
			Assert.AreEqual("F1E2D3C4 00000000 00".LogToArray(), r.Read(9));
			Assert.AreEqual("000002 000186A0 0000".LogToArray(), r.Read(9));
			Assert.AreEqual("0002 00000004".LogToArray(), r.Read(6));
		}
		
		[Test]
		public void OneByte()
		{
			TcpReader r = new TcpReader();
			
			r.AppendBlock("01020304".LogToArray());
			r.AppendBlock("05".LogToArray());
			r.AppendBlock("0607".LogToArray());
			r.PrepareRead();
			
			Assert.AreEqual(1, r.Read());
			Assert.AreEqual(2, r.Read());
			Assert.AreEqual(3, r.Read());
			Assert.AreEqual(4, r.Read());
			Assert.AreEqual(5, r.Read());
			Assert.AreEqual(6, r.Read());
			Assert.AreEqual(7, r.Read());
		}
		
		[Test]
		public void Long()
		{
			TcpReader r = new TcpReader();
			
			r.AppendBlock("F1E2D3C4 00000000 00".LogToArray());
			r.AppendBlock("000002 000186A0 0000".LogToArray());
			r.AppendBlock("0002 00000004".LogToArray());
			r.PrepareRead();
			
			Assert.AreEqual("F1E2D3C4".LogToArray(), r.Read(4));
			Assert.AreEqual("00000000 00000002 000186A0 00000002 0000".LogToArray(), r.Read(18));
			Assert.AreEqual("0004".LogToArray(), r.Read(2));
		}
	}
}

