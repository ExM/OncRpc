using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// A list of mappings
	/// http://tools.ietf.org/html/rfc1833#section-3.1
	/// </summary>
	public class pmaplist
	{
		/// <summary>
		/// mapping
		/// </summary>
		[Order(0)]
		public mapping map;
		/// <summary>
		/// next entry
		/// </summary>
		[Order(1), Option]
		public pmaplist next;
	};
}
