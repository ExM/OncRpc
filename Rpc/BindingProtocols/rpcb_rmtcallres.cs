using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Results of the remote call
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_rmtcallres
	{
		/// <summary>
		/// remote universal address
		/// </summary>
		[Order(0), Var]
		public string addr;
		/// <summary>
		/// result
		/// </summary>
		[Order(1), Var]
		public byte[] results;
	};
}
