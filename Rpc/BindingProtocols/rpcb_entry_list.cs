using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// A list of addresses supported by a service.
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_entry_list
	{
		/// <summary>
		/// contains a merged address of a service on a particular transport, plus associated netconfig information.
		/// </summary>
		[Order(0)]
		public rpcb_entry rpcb_entry_map;
		/// <summary>
		/// next node of linked list
		/// </summary>
		[Order(0), Option]
		public rpcb_entry_list rpcb_entry_next;
	};
}
