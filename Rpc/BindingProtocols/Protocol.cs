using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// constants of port mapper protocol
	/// http://tools.ietf.org/html/rfc1833#section-3.1
	/// </summary>
	public enum Protocol: int
	{
		/// <summary>
		/// protocol number for TCP/IP
		/// </summary>
		TCP = 6,
		/// <summary>
		/// protocol number for UDP/IP
		/// </summary>
		UDP = 17
	}
}
