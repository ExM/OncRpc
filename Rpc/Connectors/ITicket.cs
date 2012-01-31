using System;
using Rpc.MessageProtocol;
using Xdr;

namespace Rpc.Connectors
{
	internal interface ITicket
	{
		uint Xid { get; set; }
		void ReadResult(IMsgReader mr, Reader r, rpc_msg respMsg);
		void Except(Exception ex);
		
		void BuildRpcMessage(IByteWriter bw);
	}
}
