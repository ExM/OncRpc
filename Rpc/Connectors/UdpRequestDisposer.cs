using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rpc.Connectors
{
	internal class UdpRequestDisposer: IDisposable
	{
		private readonly AsyncUdpConnector _conn;
		private readonly IUdpReceivedHandler _handler;

		public UdpRequestDisposer(AsyncUdpConnector conn, IUdpReceivedHandler handler)
		{
			_conn = conn;
			_handler = handler;
		}

		public void Dispose()
		{
			_conn.Break(_handler);
		}
	}
}
