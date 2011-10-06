using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// results of RPCBPROC_DUMP
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcblist_ptr
	{
		/// <summary>
		///node of linked list
		/// </summary>
		[Order(0), Option]
		public rp__list Instance;
	}
}
