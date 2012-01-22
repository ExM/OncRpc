using System;
using Rpc.MessageProtocol;
using System.Threading.Tasks;
using System.Threading;

namespace Rpc
{
	/// <summary>
	/// creates tasks for the control request to the RPC server
	/// </summary>
	public interface IConnector
	{
		/// <summary>
		/// creates the task for the control request to the RPC server
		/// </summary>
		/// <returns>
		/// the runned task
		/// </returns>
		/// <param name='callBody'>call body structure</param>
		/// <param name='reqArgs'>instance of request arguments</param>
		/// <param name='options'>task creation options</param>
		/// <param name='token'>cancellation token</param>
		/// <typeparam name='TReq'>type of request</typeparam>
		/// <typeparam name='TResp'>type of response</typeparam>
		Task<TResp> CreateTask<TReq, TResp>(call_body callBody, TReq reqArgs, TaskCreationOptions options, CancellationToken token);
	}
}

