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
using System.Net;

namespace Rpc
{
	[TestFixture]
	public class TcpConnectorTest: Env
	{
		[Test]
		public void Dump()
		{
			
			IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 111);
			var conn = new RpcClient(() => new TcpSession(ep));
			
			
			
			CancellationTokenSource cts = new CancellationTokenSource();

			var t = conn.RpcBindV4(cts.Token).Dump();

			if(!t.Wait(2000))
				cts.Cancel(false);

			var list = t.Result;
			
			Assert.GreaterOrEqual(list.Count, 2);
			foreach(var item in list)
				Console.WriteLine("addr:{0} netid:{1} owner:{2} prog:{3} vers:{4}",
					item.r_addr, item.r_netid, item.r_owner, item.r_prog, item.r_vers);
		}

	}
}

