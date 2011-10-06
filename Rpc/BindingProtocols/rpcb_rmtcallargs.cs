using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Arguments of remote calls
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class rpcb_rmtcallargs
	{
		/// <summary>
		/// program number
		/// </summary>
		[Order(0)]
		public ulong prog;
		/// <summary>
		/// version number
		/// </summary>
		[Order(1)]
		public ulong vers;
		/// <summary>
		/// procedure number
		/// </summary>
		[Order(2)]
		public ulong proc;
		/// <summary>
		/// argument
		/// </summary>
		[Order(3), Var]
		public byte[] args;
	};
}
