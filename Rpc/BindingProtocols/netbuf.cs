using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// netbuf structure, used to store the transport specific form of a universal transport address.
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class netbuf
	{
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(0)]
		public uint maxlen;
		/// <summary>
		/// fixme: missing comment
		/// </summary>
		[Order(1), Var]
		public byte[] buf;
	};
}
