using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Link list of all the stats about rmtcall
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcbs_rmtcalllist
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
		public ulong proc;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(3)]
		public int success;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(4)]
		public int failure;
		/// <summary>
		/// whether callit or indirect
		/// </summary>
		[Order(5)]
		public int indirect;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(6), Var]
		public string netid;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(7), Option]
		public rpcbs_rmtcalllist next;
	};
}
