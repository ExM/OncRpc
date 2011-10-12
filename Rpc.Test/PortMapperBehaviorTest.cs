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
	public class PortMapperBehaviorTest
	{
		[Test]
		public void Dump()
		{
			var conn = new SyncUdpConnector(Config.PortMapperAddr, 2000);
			var client = new PortMapper(conn);

			client.Dump((t) =>
			{
				Assert.GreaterOrEqual(t.Count, 2);
				Assert.IsFalse(t.Any((m) => m.prot != 6 && m.prot != 17));
				Assert.IsTrue(t.Any((m) => m.port == Config.PortMapperAddr.Port && m.prog == 100000 && m.vers == 2 && m.prot == 6));
				Assert.IsTrue(t.Any((m) => m.port == Config.PortMapperAddr.Port && m.prog == 100000 && m.vers == 2 && m.prot == 17));
			}, (e) => Assert.Fail("unexpected exception: {0}", e));
		}

		[Test]
		public void Null()
		{
			var conn = new SyncUdpConnector(Config.PortMapperAddr, 2000);
			var client = new PortMapper(conn);

			bool completed = false;

			client.Null(() =>
			{
				completed = true;
			}, (e) => Assert.Fail("unexpected exception: {0}", e));

			Assert.IsTrue(completed);
		}
	}
}

