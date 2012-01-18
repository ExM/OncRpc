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
	/// The portmapper program currently supports two protocols (UDP and TCP).  The portmapper is contacted by talking to it on assigned port
	/// number 111 (SUNRPC) on either of these protocols.
	/// http://tools.ietf.org/html/rfc1833#section-3.2
	/// </summary>
	public class PortMapper : BaseTaskBuilder
	{
		/// <summary>
		/// The portmapper program currently supports two protocols (UDP and TCP).  The portmapper is contacted by talking to it on assigned port
		/// number 111 (SUNRPC) on either of these protocols.
		/// http://tools.ietf.org/html/rfc1833#section-3.2
		/// </summary>
		/// <param name="conn"></param>
		public PortMapper(IConnector conn, CancellationToken token, bool attachedToParent)
			: base(conn, token, attachedToParent)
		{
		}

		protected override uint Version
		{
			get
			{
				return 2u;
			}
		}

		/// <summary>
		/// This procedure does no work.  By convention, procedure zero of any protocol takes no parameters and returns no results.
		/// </summary>
		public Task<Xdr.Void> Null()
		{
			return CreateTask<Xdr.Void, Xdr.Void>(0u, new Xdr.Void());
		}

		/// <summary>
		/// When a program first becomes available on a machine, it registers itself with the port mapper program on the same machine.  The program
		/// passes its program number "prog", version number "vers", transport protocol number "prot", and the port "port" on which it awaits
		/// service request.
		/// The procedure returns a boolean reply whose value is "TRUE" if the procedure successfully established the mapping and
		/// "FALSE" otherwise.  The procedure refuses to establish a mapping if one already exists for the tuple "(prog, vers, prot)".
		/// </summary>
		public Task<bool> Set(mapping args)
		{
			return CreateTask<mapping, bool>(1u, args);
		}

		/// <summary>
		/// When a program becomes unavailable, it should unregister itself with the port mapper program on the same machine.  The parameters and
		/// results have meanings identical to those of "PMAPPROC_SET".  The protocol and port number fields of the argument are ignored.
		/// </summary>
		public Task<bool> UnSet(mapping args)
		{
			return CreateTask<mapping, bool>(2u, args);
		}

		/// <summary>
		/// Given a program number "prog", version number "vers", and transport protocol number "prot", this procedure returns the port number on
		/// which the program is awaiting call requests.  A port value of zeros means the program has not been registered.  The "port" field of the
		/// argument is ignored.
		/// </summary>
		public Task<uint> GetPort(mapping args)
		{
			return CreateTask<mapping, uint>(3u, args);
		}

		/// <summary>
		/// This procedure enumerates all entries in the port mapper's database.
		/// The procedure takes no parameters and returns a list of program, version, protocol, and port values.
		/// </summary>
		public Task<List<mapping>> Dump()
		{
			return CreateTask<Xdr.Void, List<mapping>>(4u, new Xdr.Void());
		}

		/// <summary>
		/// This procedure allows a client to call another remote procedure on the same machine without knowing the remote procedure's port number.
		/// It is intended for supporting broadcasts to arbitrary remote programs via the well-known port mapper's port.  The parameters "prog",
		/// "vers", "proc", and the bytes of "args" are the program number, version number, procedure number, and parameters of the remote
		/// procedure.
		/// Note:
		/// (1) This procedure only sends a reply if the procedure was successfully executed and is silent (no reply) otherwise.
		/// (2) The port mapper communicates with the remote program using UDP only.
		/// 
		/// The procedure returns the remote program's port number, and the reply is the reply of the remote procedure.
		/// </summary>
		public Task<call_result> CallIt(call_args args)
		{
			return CreateTask<call_args, call_result>(5u, args);
		}
	}
}
