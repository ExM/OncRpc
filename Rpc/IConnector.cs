using System;
using Rpc.MessageProtocol;
using System.Threading.Tasks;
using System.Threading;

namespace Rpc
{
	/// <summary>
	/// data exchange interface
	/// </summary>
	public interface IConnector
	{
		Task<TResp> CreateTask<TReq, TResp>(call_body callBody, TReq reqArgs, TaskCreationOptions options, CancellationToken token);
	}
}

