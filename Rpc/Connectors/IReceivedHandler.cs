using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;
using Xdr;

namespace Rpc.Connectors
{
	internal interface IReceivedHandler
	{
		uint Xid { get; }
		
		void ReadResult(MessageReader mr, Reader r, rpc_msg respMsg);
		
		void Except(Exception ex);

		byte[] OutBuff { get; set; }
	}
}
