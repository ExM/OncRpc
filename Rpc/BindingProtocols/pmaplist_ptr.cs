using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// A result of Dump procedure
	/// http://tools.ietf.org/html/rfc1833#section-3.1
	/// </summary>
	public struct pmaplist_ptr
	{
		/// <summary>
		/// next entry
		/// </summary>
		[Order(0), Option]
		public pmaplist next;
	};
}
