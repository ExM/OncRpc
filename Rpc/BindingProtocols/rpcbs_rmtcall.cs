using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// the stats about rmtcall
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcbs_rmtcall
	{
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(0)]
		public uint prog;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(1)]
		public uint vers;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(2)]
		public uint proc;
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
	};
}
