using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;
using System.Threading.Tasks;

namespace Rpc.BindingProtocols.TaskBuilders
{
	/// <summary>
	/// operations of binding protocols
	/// </summary>
	public abstract class BaseOperation
	{
		private const uint Program = 100000u;

		private IConnector _conn;

		internal BaseOperation(IConnector conn)
		{
			_conn = conn;
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
			return _conn.Request<TReq, TResp>(CreateHeader(proc), args);
		}
	}
}
