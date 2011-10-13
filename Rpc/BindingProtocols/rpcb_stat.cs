using System;
using Xdr;
using System.Collections.Generic;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// rpcbind statistics
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_stat
	{
		/// <summary>
		/// # of procs in rpcbind V4 plus one
		/// </summary>
		public const uint RPCBSTAT_HIGHPROC = 13;

		/// <summary>
		/// number of procedure calls by numbers
		/// </summary>
		[Order(0), Fix(RPCBSTAT_HIGHPROC)]
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
		/// list of all the stats about getport and getaddr
		/// </summary>
		[Order(3)]
		public List<rpcbs_addr> addrinfo;
		/// <summary>
		/// list of all the stats about rmtcall
		/// </summary>
		[Order(4)]
		public List<rpcbs_rmtcall> rmtinfo;
	};
}
