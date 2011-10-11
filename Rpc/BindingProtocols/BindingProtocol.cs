using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// constants of binding protocol
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public static partial class BindingProtocol
	{
		//const rpcb_highproc_2 = RPCBPROC_CALLIT;
		//const rpcb_highproc_3 = RPCBPROC_TADDR2UADDR;
		//const rpcb_highproc_4 = RPCBPROC_GETSTAT;

		/// <summary>
		/// # of procs in rpcbind V4 plus one
		/// </summary>
		public const uint RPCBSTAT_HIGHPROC = 13;
		/// <summary>
		/// provide only for rpcbind V2, V3 and V4
		/// </summary>
		public const uint RPCBVERS_STAT = 3;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		public const uint RPCBVERS_4_STAT = 2;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		public const uint RPCBVERS_3_STAT = 1;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		public const uint RPCBVERS_2_STAT = 0;
	};
}
