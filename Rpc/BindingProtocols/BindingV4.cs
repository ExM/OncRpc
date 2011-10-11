using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// rpcbind procedures
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public class BindingV4
	{
		public const uint Program = 100000u;
		public const uint Version = 4u;
		
		private IConnector _conn;
		
		public BindingV4(IConnector conn)
		{
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
			msg.body.cbody.vers = Version;
			msg.body.cbody.cred = opaque_auth.None;
			msg.body.cbody.verf = opaque_auth.None;
			
			return msg;
		}
		
		public void Set(rpcb arg, Action<bool> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(1u), arg, completed, excepted);
		}
		
		public void UnSet(rpcb arg, Action<bool> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(2u), arg, completed, excepted);
		}

		public void GetAddr(rpcb arg, Action<string> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(3u), arg, completed, excepted);
		}

		public void Dump(Action<rpcblist_ptr> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(4u), new Xdr.Void(), completed, excepted);
		}
		
		public void BCast(rpcb_rmtcallargs arg, Action<rpcb_rmtcallres> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(5u), arg, completed, excepted);
		}

		public void GetTime(Action<uint> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(6u), new Xdr.Void(), completed, excepted);
		}

		public void RPCBPROC_UADDR2TADDR(string arg, Action<netbuf> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(7u), arg, completed, excepted);
		}

		public void RPCBPROC_TADDR2UADDR(netbuf arg, Action<string> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(8u), arg, completed, excepted);
		}

		public void RPCBPROC_GETVERSADDR(rpcb arg, Action<string> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(9u), arg, completed, excepted);
		}
		
		public void RPCBPROC_INDIRECT(rpcb_rmtcallargs arg, Action<rpcb_rmtcallres> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(10u), arg, completed, excepted);
		}
		
		public void RPCBPROC_GETADDRLIST(rpcb arg, Action<rpcb_entry_list_ptr> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(11u), arg, completed, excepted);
		}

		public void RPCBPROC_GETSTAT(Action<rpcb_stat_byvers> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(12u), new Xdr.Void(), completed, excepted);
		}
	}
}
