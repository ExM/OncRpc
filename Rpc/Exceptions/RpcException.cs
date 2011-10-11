using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc
{
	/// <summary>
	/// Error associated with work on RPC protocol
	/// </summary>
	public class RpcException: SystemException
	{
		/// <summary>
		/// Error associated with work on RPC protocol
		/// </summary>
		public RpcException()
		{
		}

		/// <summary>
		/// Error associated with work on RPC protocol
		/// </summary>
		/// <param name="message"></param>
		public RpcException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Error associated with work on RPC protocol
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerEx"></param>
		public RpcException(string message, Exception innerEx)
			: base(message, innerEx)
		{
		}
	}
}
