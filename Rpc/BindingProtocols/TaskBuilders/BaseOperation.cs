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
	public abstract class BaseOperation
	{
		private const uint Program = 100000u;

		protected IConnector _conn;
		public TaskCreationOptions TaskCreationOptions { get; set; }
		public CancellationToken CancellationToken { get; set; }

		internal BaseOperation(IConnector conn)
		{
			_conn = conn;
			TaskCreationOptions = TaskCreationOptions.None;
			CancellationToken = CancellationToken.None;
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
			return _conn.CreateTask<TReq, TResp>(CreateHeader(proc), args, TaskCreationOptions, CancellationToken);
		}
	}
}
