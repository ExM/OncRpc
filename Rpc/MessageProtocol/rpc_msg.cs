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
		public body_union body;
		/// <summary>
		/// message body
		/// </summary>
		public class body_union
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
	};
}
