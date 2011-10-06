using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// results of RPCBPROC_GETADDRLIST
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_entry_list_ptr
	{
		/// <summary>
		/// next node of linked list
		/// </summary>
		[Order(0), Option]
		public rpcb_entry_list rpcb_entry_next;
	}
}
