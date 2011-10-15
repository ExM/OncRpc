using System;
using Rpc.MessageProtocol;

namespace Rpc
{
	/// <summary>
	/// data exchange interface
	/// </summary>
	public interface IConnector
	{
		/// <summary>
		/// synchronous or asynchronous execution of an RPC request
		/// </summary>
		/// <typeparam name="TReq"></typeparam>
		/// <typeparam name="TResp"></typeparam>
		/// <param name="callBody"></param>
		/// <param name="request"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		IDisposable Request<TReq, TResp>(call_body callBody, TReq request, Action<TResp> completed, Action<Exception> excepted);
	}
}

