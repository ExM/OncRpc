using System;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// message type
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public enum msg_type: int
	{
		/// <summary>
		/// call message
		/// </summary>
		CALL  = 0,
		/// <summary>
		/// reply message
		/// </summary>
		REPLY = 1
	};
}
