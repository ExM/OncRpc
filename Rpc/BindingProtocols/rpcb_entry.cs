using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// contains a merged address of a service on a particular transport, plus associated netconfig information.
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_entry
	{
		/// <summary>
		/// merged address of service
		/// </summary>
		[Order(0), Var]
		public string r_maddr;
		/// <summary>
		/// The network identifier: This is a string that represents a local identification for a network.
		/// This is defined by a system administrator based on local conventions, and cannot be depended on to have the same value on every system.
		/// </summary>
		[Order(1), Var]
		public string r_nc_netid;
		/// <summary>
		/// semantics of transport (see conctants of <see cref="Rpc.BindingProtocols.TransportSemantics"/>)
		/// </summary>
		[Order(2)]
		public ulong r_nc_semantics;
		/// <summary>
		/// protocol family (see conctants of <see cref="Rpc.BindingProtocols.ProtocolFamily"/>)
		/// </summary>
		[Order(3), Var]
		public string r_nc_protofmly;
		/// <summary>
		/// protocol name (see conctants of <see cref="Rpc.BindingProtocols.ProtocolName"/>)
		/// </summary>
		[Order(4), Var]
		public string r_nc_proto;
	};
}
