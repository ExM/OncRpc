using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		internal static ServerException SystemError()
		{
			return new ServerException();
		}

		internal static AuthenticateException AuthError(auth_stat state)
		{
			return new AuthenticateException(GetAuthDescription(state));
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

		internal static RpcException RpcVersionError(mismatch_info info)
		{
			return new RpcException(
				string.Format("unsupported RPC version number (supported versions of between {0} and {1})", info.low, info.high));
		}

		internal static RpcException ProgramMismatch(mismatch_info info)
		{
			return new RpcException(
				string.Format("remote can't support version (supported versions of between {0} and {1})", info.low, info.high));
		}

		internal static RpcException ProgramUnavalible()
		{
			return new RpcException("remote hasn't exported program");
		}

		internal static RpcException ProcedureUnavalible()
		{
			return new RpcException("program can't support procedure");
		}

		internal static RpcException GarbageArgs()
		{
			return new RpcException("procedure can't decode params");
		}
	}
}
