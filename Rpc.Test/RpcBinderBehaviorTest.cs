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
			var list = Env.CallForUdp((conn, t) => conn.RpcBindV4(t).Dump());

			Assert.GreaterOrEqual(list.Count, 2);
			foreach(var item in list)
				Console.WriteLine("addr:{0} netid:{1} owner:{2} prog:{3} vers:{4}",
					item.r_addr, item.r_netid, item.r_owner, item.r_prog, item.r_vers);
		}
		
		[Test]
		public void GetStat()
		{
			var stats = Env.CallForUdp((conn, t) => conn.RpcBindV4(t).GetStat());

			Assert.IsNotNull(stats);
		}
	}
}

