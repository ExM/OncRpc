using System;
using Xdr;

namespace Rpc.MessageProtocol
{
	/// <summary>
	/// Body of an RPC call
	/// http://tools.ietf.org/html/rfc5531#section-9
	/// </summary>
	public class call_body
	{
		/// <summary>
		/// MUST be equal to 2
		/// </summary>
		[Order(0)]
		public uint rpcvers;       /* must be equal to two (2) */
		
		/// <summary>
		/// the remote program
		/// </summary>
		[Order(1)]
		public uint prog;
		
		/// <summary>
		/// version number
		/// </summary>
		[Order(2)]
		public uint vers;
		
		/// <summary>
		/// the procedure within the remote program to be called
		/// </summary>
		[Order(3)]
		public uint proc;
		
		/// <summary>
		/// authentication credential
		/// </summary>
		[Order(4)]
		public opaque_auth cred;
		
		/// <summary>
		/// authentication verifier
		/// </summary>
		[Order(5)]
		public opaque_auth verf;
	};
}
