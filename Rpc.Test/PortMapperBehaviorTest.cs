using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Rpc.MessageProtocol;
using Xdr;
using Rpc.BindingProtocols;
using System.Linq;
using System.Threading;

namespace Rpc
{
	[TestFixture]
	public class PortMapperBehaviorTest
	{
		[Test]
		public void Dump()
		{
			var r = Env.CallForUdp((conn, t) => conn.PortMapper(t).Dump());

			Assert.GreaterOrEqual(r.Count, 2);
			Assert.IsFalse(r.Any((m) => m.prot != 6 && m.prot != 17));
			Assert.IsTrue(r.Any((m) => m.port == Env.PortMapperAddr.Port && m.prog == 100000 && m.vers == 2 && m.prot == 6));
			Assert.IsTrue(r.Any((m) => m.port == Env.PortMapperAddr.Port && m.prog == 100000 && m.vers == 2 && m.prot == 17));
		}

		[Test]
		public void Null()
		{
			var r = Env.CallForUdp((conn, t) => conn.PortMapper(t).Null());

			Assert.IsNotNull(r);
		}
	}
}

