using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// A mapping of (program, version, protocol) to port number
	/// http://tools.ietf.org/html/rfc1833#section-3.1
	/// </summary>
	public struct mapping
	{
		/// <summary>
		/// program number
		/// </summary>
		[Order(0)]
		public uint prog;
		/// <summary>
		/// version number
		/// </summary>
		[Order(1)]
		public uint vers;
		/// <summary>
		/// protocol
		/// </summary>
		[Order(2)]
		public uint prot;
		/// <summary>
		/// port
		/// </summary>
		[Order(3)]
		public uint port;

		public override string ToString()
		{
			return string.Format("port:{0} prog:{1} prot:{2} vers:{3}", port, prog, prot, vers);
		}
	};
}
