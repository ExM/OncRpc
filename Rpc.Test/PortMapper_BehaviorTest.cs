using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Rpc.MessageProtocol;
using Xdr;
using Rpc.BindingProtocols;
using System.Linq;
using System.Threading;
using Rpc.Connectors;

namespace Rpc
{
	[TestFixture("UDP")]
	[TestFixture("TCP")]
	public class PortMapper_BehaviorTest: Env
	{
		private IConnector _conn;
		
		public PortMapper_BehaviorTest(string protocolType)
		{
			if(protocolType == "TCP")
				_conn = RpcClient.FromTcp(Env.PortMapper);
			else
				_conn = RpcClient.FromUdp(Env.PortMapper);
			
		}
		
		[Test]
		public void Dump()
		{
			var r = Env.WaitTask((t) => _conn.PortMapper(t).Dump());

			Assert.GreaterOrEqual(r.Count, 2);
			Assert.IsFalse(r.Any((m) => m.prot != 6 && m.prot != 17));
			Assert.IsTrue(r.Any((m) => m.port == Env.PortMapper.Port && m.prog == 100000 && m.vers == 2 && m.prot == 6));
			Assert.IsTrue(r.Any((m) => m.port == Env.PortMapper.Port && m.prog == 100000 && m.vers == 2 && m.prot == 17));
		}

		[Test]
		public void Null()
		{
			var r = Env.WaitTask((t) => _conn.PortMapper(t).Null());
			Assert.IsInstanceOf<Xdr.Void>(r);
		}
		
		[Test]
		public void GetPort()
		{
			mapping arg = new mapping(){ prog = 100000, port = 0, prot = 6, vers = 2};
			var r = Env.WaitTask((t) => _conn.PortMapper(t).GetPort(arg));
			
			Assert.AreEqual(111, r);
		}
		
		[Test]
		public void GetPort_Fail()
		{
			mapping arg = new mapping(){ prog = 0, port = 0, prot = 17, vers = 2};
			var r = Env.WaitTask((t) => _conn.PortMapper(t).GetPort(arg));
			
			Assert.AreEqual(0, r);
		}
		
		[Test]
		[ExpectedException(typeof(AggregateException))]
		public void Set_Fail()
		{
			mapping arg = new mapping(){ prog = 100000, port = 111, prot = 6, vers = 2};
			Env.WaitTask((t) => _conn.PortMapper(t).Set(arg));
		}

	}
}

