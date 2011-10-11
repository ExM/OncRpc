using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// rpcbind procedures
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public partial class Bind
	{
		public class Version4 : DefineVersion
		{
			public override uint Version
			{
				get { return 4u; }
			}

			public readonly DefineProcedure<rpcb, bool> RPCBPROC_SET = new DefineProcedure<rpcb, bool>(1);

			public readonly DefineProcedure<rpcb, bool> RPCBPROC_UNSET = new DefineProcedure<rpcb, bool>(2);

			public readonly DefineProcedure<rpcb, string> RPCBPROC_GETADDR = new DefineProcedure<rpcb, string>(3);

			public readonly DefineProcedure<Xdr.Void, rpcblist_ptr> RPCBPROC_DUMP = new DefineProcedure<Xdr.Void, rpcblist_ptr>(4);

			public readonly DefineProcedure<rpcb_rmtcallargs, rpcb_rmtcallres> RPCBPROC_BCAST = new DefineProcedure<rpcb_rmtcallargs, rpcb_rmtcallres>(5);

			public readonly DefineProcedure<Xdr.Void, uint> RPCBPROC_GETTIME = new DefineProcedure<Xdr.Void, uint>(6);

			public readonly DefineProcedure<string, netbuf> RPCBPROC_UADDR2TADDR = new DefineProcedure<string, netbuf>(7);

			public readonly DefineProcedure<netbuf, string> RPCBPROC_TADDR2UADDR = new DefineProcedure<netbuf, string>(8);

			public readonly DefineProcedure<rpcb, string> RPCBPROC_GETVERSADDR = new DefineProcedure<rpcb, string>(9);

			public readonly DefineProcedure<rpcb_rmtcallargs, rpcb_rmtcallres> RPCBPROC_INDIRECT = new DefineProcedure<rpcb_rmtcallargs, rpcb_rmtcallres>(10);

			public readonly DefineProcedure<rpcb, rpcb_entry_list_ptr> RPCBPROC_GETADDRLIST = new DefineProcedure<rpcb, rpcb_entry_list_ptr>(11);

			public readonly DefineProcedure<Xdr.Void, rpcb_stat_byvers> RPCBPROC_GETSTAT = new DefineProcedure<Xdr.Void, rpcb_stat_byvers>(12);
		}
	}
}
