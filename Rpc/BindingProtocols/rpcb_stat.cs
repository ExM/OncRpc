using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// rpcbind statistics
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_stat
	{
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(0), Fix(BindingProtocol.RPCBSTAT_HIGHPROC)]
		public int[] info;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(1)]
		public int setinfo;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(2)]
		public int unsetinfo;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(3), Option]
		public rpcbs_addrlist addrinfo; //TODO: convert type to List<rpcbs_addr>
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(4), Option]
		public rpcbs_rmtcalllist rmtinfo;  //TODO: convert type to List<rpcbs_rmtcall>
	};
}
