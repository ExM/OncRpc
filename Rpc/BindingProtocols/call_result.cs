using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Results of callit
	/// http://tools.ietf.org/html/rfc1833#section-3.1
	/// </summary>
	public class call_result
	{
		/// <summary>
		/// port of called program
		/// </summary>
		[Order(0)]
		public uint port;

		/// <summary>
		/// result
		/// </summary>
		[Order(1), Var]
		public byte[] res;
	};
}
