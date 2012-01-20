using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Rpc.MessageProtocol;

namespace Rpc.Connectors
{
	internal interface ITicketOwner
	{
		void RemoveTicket(ITicket ticket);
	}
}
