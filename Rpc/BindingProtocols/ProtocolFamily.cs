using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Protocol family (rpcb_entry.r_nc_protofmly): This identifies the family to which the protocol belongs.
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public static class ProtocolFamily
	{
		/// <summary>
		/// no protocol family (-)
		/// NC_NOPROTOFMLY
		/// </summary>
		public const string NC_NOPROTOFMLY = "-";
		/// <summary>
		/// loopback
		/// </summary>
		public const string NC_LOOPBACK = "loopback";
		/// <summary>
		/// inet
		/// </summary>
		public const string NC_INET = "inet";
		/// <summary>
		/// implink
		/// </summary>
		public const string NC_IMPLINK = "implink";
		/// <summary>
		/// pup
		/// </summary>
		public const string NC_PUP = "pup";
		/// <summary>
		/// chaos
		/// </summary>
		public const string NC_CHAOS = "chaos";
		/// <summary>
		/// ns
		/// </summary>
		public const string NC_NS = "ns";
		/// <summary>
		/// nbs
		/// </summary>
		public const string NC_NBS = "nbs";
		/// <summary>
		/// ecma
		/// </summary>
		public const string NC_ECMA = "ecma";
		/// <summary>
		/// datakit
		/// </summary>
		public const string NC_DATAKIT = "datakit";
		/// <summary>
		/// ccitt
		/// </summary>
		public const string NC_CCITT = "ccitt";
		/// <summary>
		/// sna
		/// </summary>
		public const string NC_SNA = "sna";
		/// <summary>
		/// decnet
		/// </summary>
		public const string NC_DECNET = "decnet";
		/// <summary>
		/// dli
		/// </summary>
		public const string NC_DLI = "dli";
		/// <summary>
		/// lat
		/// </summary>
		public const string NC_LAT = "lat";
		/// <summary>
		/// hylink
		/// </summary>
		public const string NC_HYLINK = "hylink";
		/// <summary>
		/// appletalk
		/// </summary>
		public const string NC_APPLETALK = "appletalk";
		/// <summary>
		/// nit
		/// </summary>
		public const string NC_NIT = "nit";
		/// <summary>
		/// ieee802
		/// </summary>
		public const string NC_IEEE802 = "ieee802";
		/// <summary>
		/// osi
		/// </summary>
		public const string NC_OSI = "osi";
		/// <summary>
		/// x25
		/// </summary>
		public const string NC_X25 = "x25";
		/// <summary>
		/// osinet
		/// </summary>
		public const string NC_OSINET = "osinet";
		/// <summary>
		/// gosip
		/// </summary>
		public const string NC_GOSIP = "gosip";
	};
}
