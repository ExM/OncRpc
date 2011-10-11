using System;
using Rpc;
using Rpc.MessageProtocol;
using Xdr;
using System.Net.Sockets;
using System.Net;

namespace Rpc
{
	public class SyncUdpConnector: IConnector
	{
		private WriteBuilder _wb;
		private ReadBuilder _rb;
		private UdpClient _client;
		private IPEndPoint _ep;

		public SyncUdpConnector(ReadBuilder rb, WriteBuilder wb, IPEndPoint ep)
		{
			_rb = rb;
			_wb = wb;
			_ep = ep;
			_client = new UdpClient(ep.AddressFamily);
		}

		public void Request<TReq, TResp>(rpc_msg header, TReq request, Action<TResp> completed, Action<Exception> excepted)
		{
			Exception resEx = null;
			TResp respArgs = default(TResp);
			try
			{
				ByteWriter bw = new ByteWriter();
				Writer w = _wb.Create(bw);
				w.Write(header);

				byte[] outBuff = bw.ToArray();

				_client.Send(outBuff, outBuff.Length, _ep);
				IPEndPoint ep = _ep;
				byte[] inBuff = _client.Receive(ref ep);

				ByteReader br = new ByteReader(inBuff);
				Reader r = _rb.Create(br);

				rpc_msg respMsg = r.Read<rpc_msg>();

				resEx = Toolkit.ReplyMessageValidate(respMsg);
				if (resEx == null)
					respArgs = r.Read<TResp>();
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

