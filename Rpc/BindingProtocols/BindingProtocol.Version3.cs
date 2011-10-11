using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// rpcbind procedures
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public static partial class BindingProtocol
	{
		public static class Version3
		{
			public const uint Version = 3u;

			public static readonly ProcedureDescription<rpcb, bool> RPCBPROC_SET =
				new ProcedureDescription<rpcb, bool>(Program, Version, 1);

			public static readonly ProcedureDescription<rpcb, bool> RPCBPROC_UNSET =
				new ProcedureDescription<rpcb, bool>(Program, Version, 2);

			public static readonly ProcedureDescription<rpcb, string> RPCBPROC_GETADDR =
				new ProcedureDescription<rpcb, string>(Program, Version, 3);

			public static readonly ProcedureDescription<Xdr.Void, rpcblist_ptr> RPCBPROC_DUMP =
				new ProcedureDescription<Xdr.Void, rpcblist_ptr>(Program, Version, 4);

			public static readonly ProcedureDescription<rpcb_rmtcallargs, rpcb_rmtcallres> RPCBPROC_CALLIT =
				new ProcedureDescription<rpcb_rmtcallargs, rpcb_rmtcallres>(Program, Version, 5);

			public static readonly ProcedureDescription<Xdr.Void, uint> RPCBPROC_GETTIME =
				new ProcedureDescription<Xdr.Void, uint>(Program, Version, 6);

			public static readonly ProcedureDescription<string, netbuf> RPCBPROC_UADDR2TADDR =
				new ProcedureDescription<string, netbuf>(Program, Version, 7);

			public static readonly ProcedureDescription<netbuf, string> RPCBPROC_TADDR2UADDR =
				new ProcedureDescription<netbuf, string>(Program, Version, 8);
		}
	}
}
