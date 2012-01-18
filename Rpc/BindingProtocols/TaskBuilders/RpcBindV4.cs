using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;
using System.Threading.Tasks;

namespace Rpc.BindingProtocols.TaskBuilders
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
			:base(conn)
		{
		}

		protected override uint Version
		{
			get
			{
				return 4u;
			}
		}
		
		/// <summary>
		/// This procedure is identical to the version 3 RPCBPROC_CALLIT procedure.  The new name indicates that the procedure should be used
		/// for broadcast RPCs only.  RPCBPROC_INDIRECT, defined below, should be used for indirect RPC calls.
		/// </summary>
		public Task<rpcb_rmtcallres> BCast(rpcb_rmtcallargs arg)
		{
			return CreateTask<rpcb_rmtcallargs, rpcb_rmtcallres>(5u, arg);
		}

		/// <summary>
		/// This procedure is similar to RPCBPROC_GETADDR. The difference is the "r_vers" field of the rpcb structure can be used to specify the
		/// version of interest.  If that version is not registered, no address is returned.
		/// </summary>
		public Task<string> GetVersAddr(rpcb arg)
		{
			return CreateTask<rpcb, string>(9u, arg);
		}
		
		/// <summary>
		/// Similar to RPCBPROC_CALLIT. Instead of being silent about errors (such as the program not being registered on the system), this
		/// procedure returns an indication of the error.  This procedure should not be used for broadcast RPC. It is intended to be used with
		/// indirect RPC calls only.
		/// </summary>
		public Task<rpcb_rmtcallres> Indirect(rpcb_rmtcallargs arg)
		{
			return CreateTask<rpcb_rmtcallargs, rpcb_rmtcallres>(10u, arg);
		}
		
		/// <summary>
		/// This procedure returns a list of addresses for the given rpcb entry.
		/// The client may be able use the results to determine alternate transports that it can use to communicate with the server.
		/// </summary>
		public Task<List<rpcb_entry>> GetAddrList(rpcb arg)
		{
			return CreateTask<rpcb, List<rpcb_entry>>(11u, arg);
		}

		/// <summary>
		/// This procedure returns statistics on the activity of the RPCBIND server.  The information lists the number and kind of requests the
		/// server has received.
		/// 
		/// Note - All procedures except RPCBPROC_SET and RPCBPROC_UNSET can be called by clients running on a machine other than a machine on which
		/// RPCBIND is running.  RPCBIND only accepts RPCBPROC_SET and RPCBPROC_UNSET requests by clients running on the same machine as the
		/// RPCBIND program.
		/// </summary>
		public Task<rpcb_stat_byvers> GetStat()
		{
			return CreateTask<Xdr.Void, rpcb_stat_byvers>(12u, new Xdr.Void());
		}
	}
}
