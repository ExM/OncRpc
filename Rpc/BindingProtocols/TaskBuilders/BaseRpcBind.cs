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
	/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
	/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
	/// http://tools.ietf.org/html/rfc1833#section-2.2.1
	/// </summary>
	public abstract class BaseRpcBind : BaseTaskBuilder
	{
		internal BaseRpcBind(IConnector conn, CancellationToken token, bool attachedToParent)
			: base(conn, token, attachedToParent)
		{
		}

		/// <summary>
		/// When a program first becomes available on a machine, it registers itself with RPCBIND running on the same machine.
		/// The program passes its program number "r_prog", version number "r_vers", network identifier "r_netid", universal address "r_addr",
		/// and the owner of the service "r_owner".
		/// The procedure returns a boolean response whose value is TRUE if the procedure successfully established the mapping and FALSE otherwise.
		/// The procedure refuses to establish a mapping if one already exists for the ordered set ("r_prog", "r_vers", "r_netid").
		/// Note that neither "r_netid" nor "r_addr" can be NULL, and that "r_netid" should be a valid network identifier on the machine making the call.
		/// </summary>
		public Task<bool> Set(rpcb arg)
		{
			return CreateTask<rpcb, bool>(1u, arg);
		}
		
		/// <summary>
		/// When a program becomes unavailable, it should unregister itself with the RPCBIND program on the same machine.
		/// The parameters and results have meanings identical to those of RPCBPROC_SET.
		/// The mapping of the ("r_prog", "r_vers", "r_netid") tuple with "r_addr" is deleted.
		/// If "r_netid" is NULL, all mappings specified by the ordered set ("r_prog", "r_vers", *) and the corresponding universal addresses are deleted.
		/// Only the owner of the service or the super-user is allowed to unset a service
		/// </summary>
		public Task<bool> UnSet(rpcb arg)
		{
			return CreateTask<rpcb, bool>(2u, arg);
		}

		/// <summary>
		/// Given a program number "r_prog", version number "r_vers", and network identifier  "r_netid", this procedure returns the universal address
		/// on which the program is awaiting call requests.  The "r_netid" field of the argument is ignored and the "r_netid" is inferred from the
		/// network identifier of the transport on which the request came in.
		/// </summary>
		public Task<string> GetAddr(rpcb arg)
		{
			return CreateTask<rpcb, string>(3u, arg);
		}

		/// <summary>
		/// This procedure lists all entries in RPCBIND's database.
		/// The procedure takes no parameters and returns a list of program, version, network identifier, and universal addresses.
		/// </summary>
		public Task<List<rpcb>> Dump()
		{
			return CreateTask<Xdr.Void, List<rpcb>>(4u, new Xdr.Void());
		}

		/// <summary>
		/// This procedure returns the local time on its own machine in seconds
		/// since the midnight of the First day of January, 1970. 
		/// </summary>
		public Task<uint> GetTime()
		{
			return CreateTask<Xdr.Void, uint>(6u, new Xdr.Void());
		}

		/// <summary>
		/// This procedure converts universal addresses to transport specific addresses.
		/// </summary>
		public Task<netbuf> UAddr2TAddr(string arg)
		{
			return CreateTask<string, netbuf>(7u, arg);
		}

		/// <summary>
		/// This procedure converts transport specific addresses to universal addresses.
		/// </summary>
		public Task<string> TAddr2Uaddr(netbuf arg)
		{
			return CreateTask<netbuf, string>(8u, arg);
		}
	}
}
