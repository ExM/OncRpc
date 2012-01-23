using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc
{
	/// <summary>
	/// Authenticate error
	/// </summary>
	public class AuthenticateException: ReplyException
	{
		/// <summary>
		/// Authenticate error
		/// </summary>
		/// <param name="replyBody"></param>
		public AuthenticateException(reply_body replyBody)
			:base(replyBody)
		{
		}

		/// <summary>
		/// Authenticate error
		/// </summary>
		/// <param name="replyBody"></param>
		/// <param name="message"></param>
		public AuthenticateException(reply_body replyBody, string message)
			: base(replyBody, message)
		{
		}

		/// <summary>
		/// Authenticate error
		/// </summary>
		/// <param name="replyBody"></param>
		/// <param name="message"></param>
		/// <param name="innerEx"></param>
		public AuthenticateException(reply_body replyBody, string message, Exception innerEx)
			: base(replyBody, message, innerEx)
		{
		}
	}
}
