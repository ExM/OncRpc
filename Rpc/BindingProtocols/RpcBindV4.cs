using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// RPCBIND Version 4
	/// 
	/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
	/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
	/// http://tools.ietf.org/html/rfc1833#section-2.2.1
	/// </summary>
	public class RpcBindV4: BaseRpcBind
	{
		/// <summary>
		/// RPCBIND Version 4
		/// 
		/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
		/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
		/// http://tools.ietf.org/html/rfc1833#section-2.2.1
		/// </summary>
		/// <param name="conn"></param>
		public RpcBindV4(IConnector conn)
			:base(4u, conn)
		{
		}
		
		/// <summary>
		/// This procedure is identical to the version 3 RPCBPROC_CALLIT procedure.  The new name indicates that the procedure should be used
		/// for broadcast RPCs only.  RPCBPROC_INDIRECT, defined below, should be used for indirect RPC calls.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void BCast(rpcb_rmtcallargs arg, Action<rpcb_rmtcallres> completed, Action<Exception> excepted)
		{
			Request(5u, arg, completed, excepted);
		}

		/// <summary>
		/// This procedure is similar to RPCBPROC_GETADDR. The difference is the "r_vers" field of the rpcb structure can be used to specify the
		/// version of interest.  If that version is not registered, no address is returned.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void GetVersAddr(rpcb arg, Action<string> completed, Action<Exception> excepted)
		{
			Request(9u, arg, completed, excepted);
		}
		
		/// <summary>
		/// Similar to RPCBPROC_CALLIT. Instead of being silent about errors (such as the program not being registered on the system), this
		/// procedure returns an indication of the error.  This procedure should not be used for broadcast RPC. It is intended to be used with
		/// indirect RPC calls only.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void Indirect(rpcb_rmtcallargs arg, Action<rpcb_rmtcallres> completed, Action<Exception> excepted)
		{
			Request(10u, arg, completed, excepted);
		}
		
		/// <summary>
		/// This procedure returns a list of addresses for the given rpcb entry.
		/// The client may be able use the results to determine alternate transports that it can use to communicate with the server.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void GetAddrList(rpcb arg, Action<List<rpcb_entry>> completed, Action<Exception> excepted)
		{
			Request(11u, arg, completed, excepted);
		}

		/// <summary>
		/// This procedure returns statistics on the activity of the RPCBIND server.  The information lists the number and kind of requests the
		/// server has received.
		/// 
		/// Note - All procedures except RPCBPROC_SET and RPCBPROC_UNSET can be called by clients running on a machine other than a machine on which
		/// RPCBIND is running.  RPCBIND only accepts RPCBPROC_SET and RPCBPROC_UNSET requests by clients running on the same machine as the
		/// RPCBIND program.
		/// </summary>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void GetStat(Action<rpcb_stat_byvers> completed, Action<Exception> excepted)
		{
			Request(12u, new Xdr.Void(), completed, excepted);
		}
	}
}
