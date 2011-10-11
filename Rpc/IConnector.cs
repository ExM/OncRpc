using System;
using Rpc.MessageProtocol;

namespace Rpc
{
	public interface IConnector
	{
		void Request<TReq, TResp>(rpc_msg header, TReq request, Action<TResp> completed, Action<Exception> excepted);
	}
}

