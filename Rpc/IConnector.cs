using System;
using Rpc.MessageProtocol;
using System.Threading.Tasks;

namespace Rpc
{
	/// <summary>
	/// data exchange interface
	/// </summary>
	public interface IConnector
	{
		Task<TResp> Request<TReq, TResp>(call_body callBody, TReq reqArgs);
	}
}

