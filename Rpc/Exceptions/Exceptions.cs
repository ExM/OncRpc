using System;
using Rpc.MessageProtocol;

namespace Rpc
{
	internal static class Exceptions
	{
		internal static FormatException UnexpectedMessageType(msg_type present)
		{
			return new FormatException(string.Format("unexpected message type: `{0}'", present));
		}

		internal static ArgumentException NoRFC5531(string paramName)
		{
			return new ArgumentException("structure must be RFC5531", paramName);
		}

		internal static FormatException Format(string frm, params object[] args)
		{
			return new FormatException(string.Format(frm, args));
		}

		internal static ReplyException SystemError(reply_body replyBody)
		{
			return new ReplyException(replyBody, "system error in RPC-server");
		}

		internal static AuthenticateException AuthError(reply_body replyBody, auth_stat state)
		{
			return new AuthenticateException(replyBody, GetAuthDescription(state));
		}

		internal static string GetAuthDescription(auth_stat state)
		{
			switch(state)
			{
				case auth_stat.AUTH_BADCRED:
					return "bad credential (seal broken)";
				case auth_stat.AUTH_BADVERF:
					return "bad verifier (seal broken)";
				case auth_stat.AUTH_FAILED:
					return "unknown reason";
				case auth_stat.AUTH_INVALIDRESP:
					return "bogus response verifier";
				case auth_stat.AUTH_OK:
					return "success";
				case auth_stat.AUTH_REJECTEDCRED:
					return "client must begin new session";
				case auth_stat.AUTH_REJECTEDVERF:
					return "verifier expired or replayed";
				case auth_stat.AUTH_TOOWEAK:
					return "rejected for security reasons";
				case auth_stat.RPCSEC_GSS_CREDPROBLEM:
					return "no credentials for user";
				case auth_stat.RPCSEC_GSS_CTXPROBLEM:
					return "problem with context";
				default:
					return string.Format("unknown state: {0}", state);
			}
		}

		internal static ReplyException RpcVersionError(reply_body replyBody, mismatch_info info)
		{
			return new ReplyException(replyBody,
				string.Format("unsupported RPC version number (supported versions of between {0} and {1})", info.low, info.high));
		}

		internal static ReplyException ProgramMismatch(reply_body replyBody, mismatch_info info)
		{
			return new ReplyException(replyBody,
				string.Format("remote can't support program version (supported versions of between {0} and {1})", info.low, info.high));
		}

		internal static ReplyException ProgramUnavalible(reply_body replyBody)
		{
			return new ReplyException(replyBody, "remote hasn't exported program");
		}

		internal static RpcException ProcedureUnavalible(reply_body replyBody)
		{
			return new ReplyException(replyBody, "program can't support procedure");
		}

		internal static RpcException GarbageArgs()
		{
			return new RpcException("procedure can't decode params");
		}
	}
}
