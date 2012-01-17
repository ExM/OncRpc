using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Rpc.MessageProtocol;
using System.Threading.Tasks;

namespace Rpc.Connectors
{
	internal class Ticket<TReq, TResp> : ITicket
	{
		public TaskCompletionSource<TResp> TaskSrc { get; set; }
		public call_body CallBody { get; set; }
		public TReq Request { get; set; }
		public uint Xid { get; set; }

		public byte[] BuildDatagram()
		{
			rpc_msg reqHeader = new rpc_msg()
			{
				xid = Xid,
				body = new body()
				{
					mtype = msg_type.CALL,
					cbody = CallBody
				}
			};

			UdpDatagram dtg = new UdpDatagram();
			Writer w = Toolkit.CreateWriter(dtg);
			w.Write(reqHeader);
			w.Write(Request);

			CallBody = null; 
			Request = default(TReq);

			return dtg.ToArray();
		}
		
		public void ReadResult(MessageReader mr, Reader r, rpc_msg respMsg)
		{
			try
			{
				Toolkit.ReplyMessageValidate2(respMsg);

				TResp respArgs = r.Read<TResp>();
				mr.CheckEmpty();

				TaskSrc.TrySetResult(respArgs);
			}
			catch (Exception ex)
			{
				TaskSrc.TrySetException(ex);
			}
		}

		public void Except(Exception ex)
		{
			TaskSrc.TrySetException(ex);
		}
	}
}
