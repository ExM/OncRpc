using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Transport semantics (rpcb_entry.r_nc_semantics): This represents the type of transport.
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public static class TransportSemantics
	{
		/// <summary>
		/// Connectionless
		/// </summary>
		public const ulong NC_TPI_CLTS  = 1;
		/// <summary>
		/// Connection oriented
		/// </summary>
		public const ulong NC_TPI_COTS = 2;
		/// <summary>
		/// Connection oriented with graceful close
		/// </summary>
		public const ulong NC_TPI_COTS_ORD = 3;
		/// <summary>
		/// Raw transport
		/// </summary>
		public const ulong NC_TPI_RAW = 4;
	};
}
