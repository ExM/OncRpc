using System;
using Xdr;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// the lowest and highest version numbers of the remote program supported by the server
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public class mismatch_info
	{
		/// <summary>
		/// lowest version number
		/// </summary>
		[Order(0)]
		public uint low;
		
		/// <summary>
		/// highest version number
		/// </summary>
		[Order(1)]
		public uint high;
	};
}
