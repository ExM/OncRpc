using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Rpc.MessageProtocol;
using Rpc.TcpStreaming;
using Rpc.UdpDatagrams;

namespace Rpc
{
	public interface IMsgReader
	{
		void CheckEmpty();
	}
}
