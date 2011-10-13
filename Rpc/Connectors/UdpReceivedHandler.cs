using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rpc.Connectors
{
	internal class UdpReceivedHandler
	{
		public readonly uint Xid;

		public UdpReceivedHandler(uint xid)
		{
			Xid = xid;
		}
	}
}
