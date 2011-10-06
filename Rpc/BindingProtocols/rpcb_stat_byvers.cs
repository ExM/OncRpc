using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// One rpcb_stat structure is returned for each version of rpcbind being monitored.
	/// typedef rpcb_stat rpcb_stat_byvers[RPCBVERS_STAT];
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_stat_byvers
	{
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(0), Fix(BindingProtocol.RPCBVERS_STAT)]
		public rpcb_stat[] rpcb_stats;
	};
}
