using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Rpc.MessageProtocol;
using Xdr;
using Rpc.BindingProtocols;
using System.Linq;

namespace Rpc
{
	[TestFixture]
	public class RpcBinderBehaviorTest
	{
		[Test]
		public void Dump()
		{
			var conn = new SyncUdpConnector(Config.PortMapperAddr, 2000);
			var client = new RpcBindV4(conn);

			client.Dump((list) =>
			{
				Assert.GreaterOrEqual(list.Count, 2);
				foreach(var item in list)
					Console.WriteLine("addr:{0} netid:{1} owner:{2} prog:{3} vers:{4}",
						item.r_addr, item.r_netid, item.r_owner, item.r_prog, item.r_vers);
			}, (e) => Assert.Fail("unexpected exception: {0}", e));
		}
		
		[Test]
		public void GetStat()
		{
			var conn = new SyncUdpConnector(Config.PortMapperAddr, 2000);
			var client = new RpcBindV4(conn);
			
			rpcb_stat_byvers res = null;
			
			client.GetStat((stats) =>
			{
				Assert.IsNotNull(stats);

			}, (e) => Assert.Fail("unexpected exception: {0}", e));
		}
	}
}

