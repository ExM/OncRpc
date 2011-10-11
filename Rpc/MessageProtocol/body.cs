using System;
using Xdr;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// message body
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public class body
	{
		/// <summary>
		/// message type
		/// </summary>
		[Switch]
		public msg_type mtype;
		/// <summary>
		/// call body
		/// </summary>
		[Case(msg_type.CALL)]
		public call_body cbody;
		/// <summary>
		/// reply body
		/// </summary>
		[Case(msg_type.REPLY)]
		public reply_body rbody;
	}
}
