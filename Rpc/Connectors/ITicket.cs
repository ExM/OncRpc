using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Rpc.MessageProtocol;
using Rpc.TcpStreaming;

namespace Rpc.Connectors
{
	internal interface ITicket
	{
		uint Xid { get; set; }
		void ReadResult(MessageReader mr, Reader r, rpc_msg respMsg);
		void ReadResult(TcpReader mr, Reader r, rpc_msg respMsg);
		void Except(Exception ex);
		byte[] BuildUdpDatagram();
		LinkedList<byte[]> BuildTcpMessage(int maxBlock);
	}
}
