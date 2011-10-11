using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
	/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
	/// http://tools.ietf.org/html/rfc1833#section-2.2.1
	/// </summary>
	public class BaseRpcBind
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
		
		internal BaseRpcBind(uint vers, IConnector conn)
		{
			_version = vers;
			_conn = conn;
		}

		/// <summary>
		/// create header of RPC message (CALL)
		/// </summary>
		/// <param name="proc"></param>
		/// <returns></returns>
		protected rpc_msg CreateHeader(uint proc)
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
		/// When a program first becomes available on a machine, it registers itself with RPCBIND running on the same machine.
		/// The program passes its program number "r_prog", version number "r_vers", network identifier "r_netid", universal address "r_addr",
		/// and the owner of the service "r_owner".
		/// The procedure returns a boolean response whose value is TRUE if the procedure successfully established the mapping and FALSE otherwise.
		/// The procedure refuses to establish a mapping if one already exists for the ordered set ("r_prog", "r_vers", "r_netid").
		/// Note that neither "r_netid" nor "r_addr" can be NULL, and that "r_netid" should be a valid network identifier on the machine making the call.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void Set(rpcb arg, Action<bool> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(1u), arg, completed, excepted);
		}
		
		/// <summary>
		/// When a program becomes unavailable, it should unregister itself with the RPCBIND program on the same machine.
		/// The parameters and results have meanings identical to those of RPCBPROC_SET.
		/// The mapping of the ("r_prog", "r_vers", "r_netid") tuple with "r_addr" is deleted.
		/// If "r_netid" is NULL, all mappings specified by the ordered set ("r_prog", "r_vers", *) and the corresponding universal addresses are deleted.
		/// Only the owner of the service or the super-user is allowed to unset a service
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void UnSet(rpcb arg, Action<bool> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(2u), arg, completed, excepted);
		}

		/// <summary>
		/// Given a program number "r_prog", version number "r_vers", and network identifier  "r_netid", this procedure returns the universal address
		/// on which the program is awaiting call requests.  The "r_netid" field of the argument is ignored and the "r_netid" is inferred from the
		/// network identifier of the transport on which the request came in.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void GetAddr(rpcb arg, Action<string> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(3u), arg, completed, excepted);
		}

		/// <summary>
		/// This procedure lists all entries in RPCBIND's database.
		/// The procedure takes no parameters and returns a list of program, version, network identifier, and universal addresses.
		/// </summary>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void Dump(Action<rpcblist_ptr> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(4u), new Xdr.Void(), completed, excepted);
		}

		/// <summary>
		/// This procedure returns the local time on its own machine in seconds
		/// since the midnight of the First day of January, 1970. 
		/// </summary>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void GetTime(Action<uint> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(6u), new Xdr.Void(), completed, excepted);
		}

		/// <summary>
		/// This procedure converts universal addresses to transport specific addresses.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void UAddr2TAddr(string arg, Action<netbuf> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(7u), arg, completed, excepted);
		}

		/// <summary>
		/// This procedure converts transport specific addresses to universal addresses.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void TAddr2Uaddr(netbuf arg, Action<string> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(8u), arg, completed, excepted);
		}
	}
}
