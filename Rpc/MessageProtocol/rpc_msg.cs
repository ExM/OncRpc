using System;
using Xdr;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// RPC message
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public class rpc_msg
	{
		/// <summary>
		/// transaction identifier
		/// </summary>
		[Order(0)]
		public uint xid;
		/// <summary>
		/// message body
		/// </summary>
		[Order(1)]
		public body body;
	};
}
