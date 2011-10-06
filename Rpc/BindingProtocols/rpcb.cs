using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// A mapping of (program, version, network ID) to address.
	/// The network identifier (r_netid): This is a string that represents a local identification for a network.
	/// This is defined by a system administrator based on local conventions, and cannot be depended on to have
	/// the same value on every system.
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public struct rpcb
	{
		/// <summary>
		/// program number
		/// </summary>
		[Order(0)]
		public ulong r_prog;
		/// <summary>
		/// version number
		/// </summary>
		[Order(1)]
		public ulong r_vers;
		/// <summary>
		/// network id 
		/// </summary>
		[Order(2), Var]
		public string r_netid;
		/// <summary>
		/// universal address
		/// </summary>
		[Order(3), Var]
		public string r_addr;
		/// <summary>
		/// owner of this service
		/// </summary>
		[Order(4), Var]
		public string r_owner;
	};
}
