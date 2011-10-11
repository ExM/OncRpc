using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc
{
	/// <summary>
	/// Error received in response RPC message
	/// </summary>
	public class ReplyException: RpcException
	{
		/// <summary>
		/// Body of a reply to an RPC call for details
		/// </summary>
		public reply_body ReplyBody { get; private set; }

		/// <summary>
		/// Error received in response RPC message
		/// </summary>
		/// <param name="replyBody"></param>
		public ReplyException(reply_body replyBody)
		{
			ReplyBody = replyBody;
		}

		/// <summary>
		/// Error received in response RPC message
		/// </summary>
		/// <param name="replyBody"></param>
		/// <param name="message"></param>
		public ReplyException(reply_body replyBody, string message)
			: base(message)
		{
			ReplyBody = replyBody;
		}

		/// <summary>
		/// Error received in response RPC message
		/// </summary>
		/// <param name="replyBody"></param>
		/// <param name="message"></param>
		/// <param name="innerEx"></param>
		public ReplyException(reply_body replyBody, string message, Exception innerEx)
			: base(message, innerEx)
		{
			ReplyBody = replyBody;
		}
	}
}
