using System;
using Xdr;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// Reply to an RPC call that was rejected by the server
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public class rejected_reply
	{
		/// <summary>
		/// Reasons why a call message was rejected
		/// </summary>
		[Switch]
		public reject_stat rstat;
		
		/// <summary>
		/// the lowest and highest version numbers of the remote program supported by the server
		/// </summary>
		[Case(reject_stat.RPC_MISMATCH)]
		public mismatch_info mismatch_info;
		
		/// <summary>
		/// the server rejects the identity of the caller
		/// </summary>
		[Case(reject_stat.AUTH_ERROR)]
		public auth_stat astat;
	};
}
