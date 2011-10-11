using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// RPCBIND Version 3
	/// 
	/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
	/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
	/// http://tools.ietf.org/html/rfc1833#section-2.2.1
	/// </summary>
	public class RpcBindV3 : BaseRpcBind
	{
		/// <summary>
		/// RPCBIND Version 3
		/// 
		/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
		/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
		/// http://tools.ietf.org/html/rfc1833#section-2.2.1
		/// </summary>
		/// <param name="conn"></param>
		public RpcBindV3(IConnector conn)
			:base(3u, conn)
		{
		}

		/// <summary>
		/// This procedure allows a caller to call another remote procedure on the same machine without knowing the remote procedure's universal
		/// address.  It is intended for supporting broadcasts to arbitrary remote programs via RPCBIND's universal address.  The parameters
		/// "prog", "vers", "proc", and args are the program number, version number, procedure number, and parameters of the remote procedure.
		/// Note - This procedure only sends a response if the procedure was successfully executed and is silent (no response) otherwise.
		/// The procedure returns the remote program's universal address, and the results of the remote procedure.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void CallIt(rpcb_rmtcallargs arg, Action<rpcb_rmtcallres> completed, Action<Exception> excepted)
		{
			_conn.Request(CreateHeader(5u), arg, completed, excepted);
		}
	}
}
