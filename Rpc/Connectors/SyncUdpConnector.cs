using System;
using Rpc;
using Rpc.MessageProtocol;
using Xdr;
using System.Net.Sockets;
using System.Net;

namespace Rpc
{
	/// <summary>
	/// connector on the UDP with a synchronous query execution
	/// </summary>
	public class SyncUdpConnector: IConnector
	{
		private WriteBuilder _wb;
		private ReadBuilder _rb;
		private UdpClient _client;
		private IPEndPoint _ep;

		/// <summary>
		/// connector on the UDP with a synchronous query execution
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="wb"></param>
		/// <param name="ep"></param>
		/// <param name="timeout"></param>
		public SyncUdpConnector(ReadBuilder rb, WriteBuilder wb, IPEndPoint ep, int timeout)
		{
			_rb = rb;
			_wb = wb;
			_ep = ep;
			_client = new UdpClient(ep.AddressFamily);
			_client.Client.ReceiveTimeout = timeout;
		}

		/// <summary>
		/// synchronous execution of an RPC request
		/// </summary>
		/// <typeparam name="TReq"></typeparam>
		/// <typeparam name="TResp"></typeparam>
		/// <param name="reqHeader"></param>
		/// <param name="reqArgs"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void Request<TReq, TResp>(rpc_msg reqHeader, TReq reqArgs, Action<TResp> completed, Action<Exception> excepted)
		{
			Exception resEx = null;
			TResp respArgs = default(TResp);
			try
			{
				reqHeader.xid = 0xF1E2D3C4;
				
				UdpDatagram dtg = new UdpDatagram();
				Writer w = _wb.Create(dtg);
				w.Write(reqHeader);
				w.Write(reqArgs);

				byte[] outBuff = dtg.ToArray();

				_client.Send(outBuff, outBuff.Length, _ep);
				MessageReader br;
				rpc_msg respMsg;
				Reader r;
				while(true)
				{
					IPEndPoint ep = _ep;
					byte[] inBuff = _client.Receive(ref ep); //TODO: calc sum time out
	
					br = new MessageReader(inBuff);
					r = _rb.Create(br);
	
					respMsg = r.Read<rpc_msg>();
					if(respMsg.xid == reqHeader.xid)
						break;
				}
				
				resEx = Toolkit.ReplyMessageValidate(respMsg);
				if (resEx == null)
				{
					respArgs = r.Read<TResp>();
					br.CheckEmpty();
				}
			}
			catch (Exception ex)
			{
				resEx = new RpcException("request error", ex); //FIXME: may be to add more context of header
			}

			if (resEx == null)
				completed(respArgs);
			else
				excepted(resEx);
		}
	}
}

