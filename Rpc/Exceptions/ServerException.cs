using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc
{
	public class ServerException: RpcException
	{
		public ServerException()
			: base("system error in RPC-server")
		{
		}

		public ServerException(string message)
			: base(message)
		{
		}

		public ServerException(string message, Exception innerEx)
			: base(message, innerEx)
		{
		}
	}
}
