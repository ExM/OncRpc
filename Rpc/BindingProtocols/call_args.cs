using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// arguments to callit
	/// http://tools.ietf.org/html/rfc1833#section-3.1
	/// </summary>
	public class call_args
	{
		/// <summary>
		/// program
		/// </summary>
		[Order(0)]
		public uint prog;

		/// <summary>
		/// version
		/// </summary>
		[Order(1)]
		public uint vers;

		/// <summary>
		/// procedure
		/// </summary>
		[Order(2)]
		public uint proc;

		/// <summary>
		/// arguments
		/// </summary>
		[Order(3), Var]
		public byte[] args;
	};
}
