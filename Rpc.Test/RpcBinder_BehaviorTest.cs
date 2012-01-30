using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Rpc.MessageProtocol;
using Xdr;
using Rpc.BindingProtocols;
using System.Linq;
using Rpc.Connectors;
using System.Threading;

namespace Rpc
{
	[TestFixture("UDP")]
	[TestFixture("TCP")]
	public class RpcBindV4_BehaviorTest: Env
	{
		private IConnector _conn;
		
		public RpcBindV4_BehaviorTest(string protocolType)
		{
			if(protocolType == "TCP")
				_conn = RpcClient.FromTcp(Env.PortMapper);
			else
				_conn = RpcClient.FromUdp(Env.PortMapper);
		}
		
		[Test]
		public void Dump()
		{
			var list = Env.WaitTask(t => _conn.RpcBindV4(t).Dump());

			Assert.GreaterOrEqual(list.Count, 3);

			//foreach(var item in list)
			//	Console.WriteLine("addr:{0} netid:{1} owner:{2} prog:{3} vers:{4}",
			//		item.r_addr, item.r_netid, item.r_owner, item.r_prog, item.r_vers);
			
			uint[] vers = list.Where((m) => 
				m.r_addr.EndsWith(".0.111") &&
				(m.r_netid == "tcp" || m.r_netid == "udp") &&
				m.r_prog == 100000).Select(m => m.r_vers).ToArray();
			
			Assert.Contains(2, vers);
			Assert.Contains(3, vers);
			Assert.Contains(4, vers);
		}
		
		[Test]
		public void GetStat()
		{
			var stats1 = Env.WaitTask(t => _conn.RpcBindV4(t).GetStat());
			var stats2 = Env.WaitTask(t => _conn.RpcBindV4(t).GetStat());
			
			Assert.Greater(stats2.V4.info[12], stats1.V4.info[12]);
		}
		
		[Test]
		public void GetTime()
		{
			var t1 = Env.WaitTask(t => _conn.RpcBindV4(t).GetTime());
			Thread.Sleep(1500);
			var t2 = Env.WaitTask(t => _conn.RpcBindV4(t).GetTime());
			
			Assert.Greater(t2, t1);
		}
	}
}

