using System;
using Xdr;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// Reply to an RPC call that was accepted by the server
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public class accepted_reply
	{
		/// <summary>
		/// authentication verifier that the server generates in order to validate itself to the client
		/// </summary>
		[Order(0)]
		public opaque_auth verf;

		/// <summary>
		/// the reply data.
		/// </summary>
		[Order(1)]
		public reply_data_union reply_data;
		
		/// <summary>
		/// the reply data
		/// </summary>
		public class reply_data_union
		{
			/// <summary>
			/// accept state
			/// </summary>
			[Switch]
			[Case(accept_stat.SUCCESS)] // opaque results[0]; -  procedure-specific results start here
			[Case(accept_stat.PROG_UNAVAIL), Case(accept_stat.PROC_UNAVAIL), Case(accept_stat.GARBAGE_ARGS), Case(accept_stat.SYSTEM_ERR)] // void
			public accept_stat stat;
			
			/// <summary>
			/// the lowest and highest version numbers of the remote program supported by the server
			/// </summary>
			[Case(accept_stat.PROG_MISMATCH)]
			public mismatch_info mismatch_info;
		}
	};
}
