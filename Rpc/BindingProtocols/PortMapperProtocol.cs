using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// constants of port mapper protocol
	/// http://tools.ietf.org/html/rfc1833#section-3.1
	/// </summary>
	public static class PortMapperProtocol
	{
		/// <summary>
		/// protocol number for TCP/IP
		/// </summary>
		public const uint IPPROTO_TCP = 6;

		/// <summary>
		/// protocol number for UDP/IP
		/// </summary>
		public const uint IPPROTO_UDP = 17;
	}
}
