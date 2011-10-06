using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	
	public static class BindingProtocol
	{
		//const rpcb_highproc_2 = RPCBPROC_CALLIT;
		//const rpcb_highproc_3 = RPCBPROC_TADDR2UADDR;
		//const rpcb_highproc_4 = RPCBPROC_GETSTAT;

		public const uint RPCBSTAT_HIGHPROC = 13; /* # of procs in rpcbind V4 plus one */
		public const uint RPCBVERS_STAT     = 3; /* provide only for rpcbind V2, V3 and V4 */
		public const uint RPCBVERS_4_STAT   = 2;
		public const uint RPCBVERS_3_STAT   = 1;
		public const uint RPCBVERS_2_STAT   = 0;
	};
}
