using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;
using System.Threading.Tasks;
using System.Threading;

namespace Rpc.BindingProtocols.TaskBuilders
{
	/// <summary>
	/// operations of binding protocols
	/// </summary>
	public abstract class BaseTaskBuilder
	{
		private const uint Program = 100000u;

		private IConnector _conn;
		private bool _attachedToParent;
		private CancellationToken _token;

		internal BaseTaskBuilder(IConnector conn, CancellationToken token, bool attachedToParent)
		{
			_conn = conn;
			_attachedToParent = attachedToParent;
			_token = token;
		}
		
		/// <summary>
		/// Gets the version of protocol
		/// </summary>
		protected abstract uint Version { get; }

		private call_body CreateHeader(uint procNum)
		{
			return new call_body()
			{
				rpcvers = 2,
				prog = Program,
				proc = procNum,
				vers = Version,
				cred = opaque_auth.None,
				verf = opaque_auth.None
			};
		}
		
		/// <summary>
		/// Creates the task of request.
		/// </summary>
		/// <returns>
		/// The queued task.
		/// </returns>
		/// <param name='proc'>
		/// procedure number
		/// </param>
		/// <param name='args'>
		/// instance of arguments of request
		/// </param>
		/// <typeparam name='TReq'>
		/// type of request
		/// </typeparam>
		/// <typeparam name='TResp'>
		/// type of response
		/// </typeparam>
		protected Task<TResp> CreateTask<TReq, TResp>(uint proc, TReq args)
		{
			return _conn.CreateTask<TReq, TResp>(CreateHeader(proc), args,
				_attachedToParent ? TaskCreationOptions.AttachedToParent : TaskCreationOptions.None, _token);
		}
	}
}
