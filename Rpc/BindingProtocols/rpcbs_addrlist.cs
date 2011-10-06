using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Link list of all the stats about getport and getaddr
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcbs_addrlist
	{
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(0)]
		public ulong prog;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(1)]
		public ulong vers;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(2)]
		public int success;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(3)]
		public int failure;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(4), Var]
		public string netid;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(5), Option]
		public rpcbs_addrlist next;
	};
}
