using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// One rpcb_stat structure is returned for each version of rpcbind being monitored.
	/// Provide only for rpcbind V2, V3 and V4.
	/// typedef rpcb_stat rpcb_stat_byvers[RPCBVERS_STAT];
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_stat_byvers
	{
		/// <summary>
		/// rpcbind V2 statistics
		/// </summary>
		[Order(0)]
		public rpcb_stat V2;
		/// <summary>
		/// rpcbind V3 statistics
		/// </summary>
		[Order(1)]
		public rpcb_stat V3;
		/// <summary>
		/// rpcbind V4 statistics
		/// </summary>
		[Order(2)]
		public rpcb_stat V4;
	};
}
