using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;
using Xdr;
using System.IO;
using System.Threading;

namespace Rpc
{
	/// <summary>
	/// set of tools to work with RPC messages
	/// </summary>
	public static class Toolkit
	{
		private static readonly WriteBuilder _wb;
		private static readonly ReadBuilder _rb;

		static Toolkit()
		{
			_wb = new WriteBuilder();
			_rb = new ReadBuilder();
		}

		/// <summary>
		/// create writer configured for RPC protocol
		/// </summary>
		/// <param name="writer"></param>
		/// <returns></returns>
		public static Writer CreateWriter(IByteWriter writer)
		{
			return _wb.Create(writer);
		}

		/// <summary>
		/// create reader configured for RPC protocol
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static Reader CreateReader(IByteReader reader)
		{
			return _rb.Create(reader);
		}
		
		/// <summary>
		/// To create a delegate output byte array to the log
		/// </summary>
		internal static string DumpToLog(string frm, byte[] buffer)
		{
			return string.Format(frm, buffer.ToDisplay());
		}

		/// <summary>
		/// convert byte array to text
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static string ToDisplay(this byte[] buffer)
		{
			// example:
			// 12345678-12345678-12345678-12345678-12345678-12345678-12345678-12345678 12345678-1234...

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < buffer.Length; i++)
			{
				if (i % 4 == 0)
				{
					if (i % 32 == 0)
						sb.AppendLine();
					else
						sb.Append(' ');
				}

				sb.Append(buffer[i].ToString("X2"));
			}

			return sb.ToString();
		}

		/// <summary>
		/// returns the description of the RPC message
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static void ReplyMessageValidate(rpc_msg msg)
		{
			try
			{
				if (msg.body.mtype != msg_type.REPLY)
					throw Exceptions.UnexpectedMessageType(msg.body.mtype);

				reply_body replyBody = msg.body.rbody;

				if (replyBody.stat == reply_stat.MSG_ACCEPTED)
				{
					accepted_reply.reply_data_union du = replyBody.areply.reply_data;
					switch (du.stat)
					{
						case accept_stat.GARBAGE_ARGS:
							throw Exceptions.GarbageArgs();
						case accept_stat.PROC_UNAVAIL:
							throw Exceptions.ProcedureUnavalible(replyBody);
						case accept_stat.PROG_MISMATCH:
							throw Exceptions.ProgramMismatch(replyBody, du.mismatch_info);
						case accept_stat.PROG_UNAVAIL:
							throw Exceptions.ProgramUnavalible(replyBody);
						case accept_stat.SUCCESS:
							return;
						case accept_stat.SYSTEM_ERR:
							throw Exceptions.SystemError(replyBody);
						default:
							throw Exceptions.NoRFC5531("msg");
					}
				}

				if (replyBody.stat == reply_stat.MSG_DENIED)
				{
					if (replyBody.rreply.rstat == reject_stat.AUTH_ERROR)
						throw Exceptions.AuthError(replyBody, replyBody.rreply.astat);
					else if (replyBody.rreply.rstat == reject_stat.RPC_MISMATCH)
						throw Exceptions.RpcVersionError(replyBody, replyBody.rreply.mismatch_info);
					else
						throw Exceptions.NoRFC5531("msg");
				}
				
				throw Exceptions.NoRFC5531("msg");
			}
			catch (NullReferenceException)
			{
				throw Exceptions.NoRFC5531("msg");
			}
		}
	}
}
