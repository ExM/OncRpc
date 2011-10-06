using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// node of linked list
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rp__list
	{
		/// <summary>
		/// A mapping of (program, version, network ID) to address.
		/// </summary>
		[Order(0)]
		public rpcb rpcb_map;
		/// <summary>
		/// next node
		/// </summary>
		[Order(1), Option]
		public rp__list rpcb_next;
	};
}
