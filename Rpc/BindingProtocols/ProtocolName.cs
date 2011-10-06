using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Protocol name (rpcb_entry.r_nc_proto): This identifies a protocol within a family.
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public static class ProtocolName
	{
		/// <summary>
		/// no protocol name (-)
		/// </summary>
		public const string NC_NOPROTO = "-";
		/// <summary>
		/// tcp
		/// </summary>
		public const string NC_TCP = "tcp";
		/// <summary>
		/// udp
		/// </summary>
		public const string NC_UDP = "udp";
		/// <summary>
		/// icmp
		/// </summary>
		public const string NC_ICMP = "icmp";
	};
}
