using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// operations of binding protocols
	/// </summary>
	public abstract class BaseOperation
	{
		/// <summary>
		/// program number
		/// </summary>
		protected const uint Program = 100000u;

		/// <summary>
		/// data exchange
		/// </summary>
		protected IConnector _conn;

		/// <summary>
		/// version number
		/// </summary>
		protected uint _version;

		internal BaseOperation(uint vers, IConnector conn)
		{
			_version = vers;
			_conn = conn;
		}

		private rpc_msg CreateHeader(uint proc)
		{
			rpc_msg msg = new rpc_msg();
			msg.xid = 0;
			msg.body = new body();
			msg.body.mtype = msg_type.CALL;
			msg.body.cbody = new call_body();
			msg.body.cbody.rpcvers = 2;
			msg.body.cbody.prog = Program;
			msg.body.cbody.proc = proc;
			msg.body.cbody.vers = _version;
			msg.body.cbody.cred = opaque_auth.None;
			msg.body.cbody.verf = opaque_auth.None;
			
			return msg;
		}

		/// <summary>
		/// generic request
		/// </summary>
		/// <typeparam name="TReq"></typeparam>
		/// <typeparam name="TResp"></typeparam>
		/// <param name="proc"></param>
		/// <param name="args"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		protected void Request<TReq, TResp>(uint proc, TReq args, Action<TResp> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(proc), args, completed, excepted);
		}
	}
}
