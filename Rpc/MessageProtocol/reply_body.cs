using System;
using Xdr;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// Body of a reply to an RPC call
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public class reply_body
	{
		/// <summary>
		/// A reply to a call message can take on two forms: the message was either accepted or rejected.
		/// </summary>
		[Switch]
		public reply_stat stat;
		
		/// <summary>
		/// Reply to an RPC call that was accepted by the server
		/// </summary>
		[Case(reply_stat.MSG_ACCEPTED)]
		public accepted_reply areply;
		
		/// <summary>
		/// Reply to an RPC call that was rejected by the server
		/// </summary>
		[Case(reply_stat.MSG_DENIED)]
		public rejected_reply rreply;
	};
}
