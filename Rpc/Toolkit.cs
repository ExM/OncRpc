using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc
{
	/// <summary>
	/// set of tools to work with RPC messages
	/// </summary>
	public static class Toolkit
	{
		/// <summary>
		/// returns the description of the RPC message
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static Exception ReplyMessageValidate(rpc_msg msg)
		{
			try
			{
				if (msg.body.mtype != msg_type.REPLY)
					return Exceptions.UnexpectedMessageType(msg.body.mtype);

				reply_body replyBody = msg.body.rbody;

				if (replyBody.stat == reply_stat.MSG_ACCEPTED)
				{
					accepted_reply.reply_data_union du = replyBody.areply.reply_data;
					switch(du.stat)
					{
						case accept_stat.GARBAGE_ARGS:
							return Exceptions.GarbageArgs();
						case accept_stat.PROC_UNAVAIL:
							return Exceptions.ProcedureUnavalible();
						case accept_stat.PROG_MISMATCH:
							return Exceptions.ProgramMismatch(du.mismatch_info);
						case accept_stat.PROG_UNAVAIL:
							return Exceptions.ProgramUnavalible();
						case accept_stat.SUCCESS:
							return null;
						case accept_stat.SYSTEM_ERR:
							return Exceptions.SystemError();
						default:
							throw null;
					}
				}
				else if(replyBody.stat == reply_stat.MSG_DENIED)
				{
					if (replyBody.rreply.rstat == reject_stat.AUTH_ERROR)
						return Exceptions.AuthError(replyBody.rreply.astat);
					else if (replyBody.rreply.rstat == reject_stat.RPC_MISMATCH)
						return Exceptions.RpcVersionError(replyBody.rreply.mismatch_info);
					else
						throw null;
				}
				else
					throw null;
			}
			catch(NullReferenceException)
			{
				throw Exceptions.NoRFC5531("msg");
			}
		}
	}
}
