using System;
using Rpc;
using Rpc.MessageProtocol;
using Xdr;
using System.Net.Sockets;
using System.Net;

namespace EmitTest
{
	public class UdpConnector: IConnector
	{
		private WriteBuilder _wb;
		private ReadBuilder _rb;
		private UdpClient _client;
		
		public UdpConnector (ReadBuilder rb, WriteBuilder wb)
		{
			_rb = rb;
			_wb = wb;
			_client = new UdpClient(AddressFamily.InterNetwork);
		}

		public void Request<TReq, TResp>(rpc_msg header, TReq request, Action<TResp> completed, Action<Exception> excepted)
		{
			ByteWriter bw = new ByteWriter();
			Writer w = _wb.Create(bw);
			w.Write(header);

			byte[] outBuff = bw.ToArray();


			IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 111);
			
			_client.Send(outBuff, outBuff.Length, ep);
			
			byte[] inBuff = _client.Receive(ref ep);
			
			ByteReader br = new ByteReader(inBuff);
			Reader r = _rb.Create(br);
			
			rpc_msg respHeader = r.Read<rpc_msg>();
			
			TResp respArgs = r.Read<TResp>();
			
			completed(respArgs);
		}
	}
}

