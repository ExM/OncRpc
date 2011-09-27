using System;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// Reasons why a call message was rejected
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public enum reject_stat: int
	{
		/// <summary>
		/// RPC version number != 2
		/// </summary>
		RPC_MISMATCH = 0,
		/// <summary>
		/// remote can't authenticate caller
		/// </summary>
		AUTH_ERROR   = 1
	};
}
