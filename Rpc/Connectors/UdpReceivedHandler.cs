using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;
using Xdr;

namespace Rpc.Connectors
{
	internal class UdpReceivedHandler<TResp>: IUdpReceivedHandler
	{
		public readonly uint _xid;
		
		private readonly Action<TResp> _completed;
		private readonly Action<Exception> _excepted;
		

		public UdpReceivedHandler(uint xid, Action<TResp> completed, Action<Exception> excepted)
		{
			_xid = xid;
			_completed = completed;
			_excepted = excepted;
		}
		
		public uint Xid
		{
			get
			{
				return _xid;
			}
		}

		public void ReadResult(MessageReader mr, Reader r, rpc_msg respMsg)
		{
			Exception resEx = null;
			TResp respArgs = default(TResp);
			
			try
			{
				
				
				resEx = Toolkit.ReplyMessageValidate(respMsg);
				if (resEx == null)
				{
					respArgs = r.Read<TResp>();
					mr.CheckEmpty();
				}
			}
			catch (Exception ex)
			{
				resEx = new RpcException("request error", ex); //FIXME: may be to add more context of header
			}

			if (resEx == null)
				_completed(respArgs);
			else
				_excepted(resEx);
		}
	}
}
