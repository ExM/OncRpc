using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Rpc.MessageProtocol;
using System.Threading;
using System.Threading.Tasks;

namespace Rpc.Connectors
{
	internal class Ticket<TReq, TResp> : ITicket
	{
		private ITicketOwner _owner;
		private call_body _callBody;
		private TReq _reqArgs;
		private TaskCompletionSource<TResp> _taskSrc;
		private CancellationTokenRegistration _ctr;

		public uint Xid { get; set; }

		public Ticket(ITicketOwner owner, call_body callBody, TReq reqArgs, TaskCreationOptions options, CancellationToken token)
		{
			_owner = owner;
			_callBody = callBody;
			_reqArgs = reqArgs;
			_taskSrc = new TaskCompletionSource<TResp>(options);
			if(token.CanBeCanceled)
				_ctr = token.Register(Cancel);
			else
				_ctr = new CancellationTokenRegistration();
		}

		public Task<TResp> Task
		{
			get
			{
				return _taskSrc.Task;
			}
		}

		public Queue<byte[]> BuildTcpMessage(int maxBlock)
		{
			//_tcpMessage = new Queue<byte[]>();

			_callBody = null;
			_reqArgs = default(TReq);

			
			//TODO: 
			throw new NotImplementedException();
		}

		public byte[] BuildUdpDatagram()
		{
			rpc_msg reqHeader = new rpc_msg()
			{
				xid = Xid,
				body = new body()
				{
					mtype = msg_type.CALL,
					cbody = _callBody
				}
			};

			UdpDatagram dtg = new UdpDatagram();
			Writer w = Toolkit.CreateWriter(dtg);
			w.Write(reqHeader);
			w.Write(_reqArgs);

			_callBody = null;
			_reqArgs = default(TReq);

			return dtg.ToArray();
		}
		
		public void ReadResult(MessageReader mr, Reader r, rpc_msg respMsg)
		{
			_ctr.Dispose();
			try
			{
				Toolkit.ReplyMessageValidate(respMsg);

				TResp respArgs = r.Read<TResp>();
				mr.CheckEmpty();

				_taskSrc.TrySetResult(respArgs);
			}
			catch (Exception ex)
			{
				_taskSrc.TrySetException(ex);
			}
		}

		public void Except(Exception ex)
		{
			_ctr.Dispose();
			_taskSrc.TrySetException(ex);
		}

		private void Cancel()
		{
			if(_taskSrc.TrySetCanceled())
				_owner.RemoveTicket(this);
		}
	}
}
