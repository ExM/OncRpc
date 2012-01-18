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

		protected IConnector _conn;
		protected bool _attachedToParent;
		protected CancellationToken _token;

		internal BaseTaskBuilder(IConnector conn, CancellationToken token, bool attachedToParent)
		{
			_conn = conn;
			_attachedToParent = attachedToParent;
			_token = token;
		}

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

		protected Task<TResp> CreateTask<TReq, TResp>(uint proc, TReq args)
		{
			return _conn.CreateTask<TReq, TResp>(CreateHeader(proc), args,
				_attachedToParent ? TaskCreationOptions.AttachedToParent : TaskCreationOptions.None, _token);
		}
	}
}
