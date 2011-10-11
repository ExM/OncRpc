using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc
{
	public class AuthenticateException: RpcException
	{
		public AuthenticateException(string message)
			: base(message)
		{
		}

		public AuthenticateException(string message, Exception innerEx)
			: base(message, innerEx)
		{
		}
	}
}
