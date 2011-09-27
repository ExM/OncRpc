using System;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// A reply to a call message can take on two forms: the message was either accepted or rejected.
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public enum reply_stat: int
	{
		/// <summary>
		/// the message was accepted
		/// </summary>
		MSG_ACCEPTED = 0,
		/// <summary>
		/// the message was rejected
		/// </summary>
		MSG_DENIED   = 1
	};
}
