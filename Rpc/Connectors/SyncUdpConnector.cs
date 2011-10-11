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
		public SyncUdpConnector(ReadBuilder rb, WriteBuilder wb, IPEndPoint ep)
		{
			_rb = rb;
			_wb = wb;
			_ep = ep;
			_client = new UdpClient(ep.AddressFamily);
		}

		/// <summary>
		/// synchronous execution of an RPC request
		/// </summary>
		/// <typeparam name="TReq"></typeparam>
		/// <typeparam name="TResp"></typeparam>
		/// <param name="header"></param>
		/// <param name="request"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void Request<TReq, TResp>(rpc_msg header, TReq request, Action<TResp> completed, Action<Exception> excepted)
		{
			Exception resEx = null;
			TResp respArgs = default(TResp);
			try
			{
				UdpDatagram dtg = new UdpDatagram();
				Writer w = _wb.Create(dtg);
				w.Write(header);
				w.Write(request);

				byte[] outBuff = dtg.ToArray();

				_client.Send(outBuff, outBuff.Length, _ep);
				IPEndPoint ep = _ep;
				byte[] inBuff = _client.Receive(ref ep); //TODO: add time out

				MessageReader br = new MessageReader(inBuff);
				Reader r = _rb.Create(br);

				rpc_msg respMsg = r.Read<rpc_msg>();

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

