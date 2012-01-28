using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Rpc.MessageProtocol;
using Rpc.TcpStreaming;
using Rpc.UdpDatagrams;

namespace Rpc.Connectors
{
	public interface ITicket
	{
		uint Xid { get; set; }
		void ReadResult(IMsgReader mr, Reader r, rpc_msg respMsg);
		void Except(Exception ex);
		
		void BuildRpcMessage(IByteWriter bw);
	}
}
