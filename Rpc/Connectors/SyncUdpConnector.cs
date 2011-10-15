using System;
using Rpc;
using Rpc.MessageProtocol;
using Xdr;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using NLog;

namespace Rpc
{
	/// <summary>
	/// connector on the UDP with a synchronous query execution
	/// </summary>
	public class SyncUdpConnector: IConnector
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private UdpClient _client;
		private IPEndPoint _ep;
		private int _timeout;

		/// <summary>
		/// connector on the UDP with a synchronous query execution
		/// </summary>
		/// <param name="ep"></param>
		/// <param name="timeout"></param>
		public SyncUdpConnector(IPEndPoint ep, int timeout)
		{
			Log.Info("create connector from {0} (to: {1} ms)", ep, timeout);
			_ep = ep;
			_client = new UdpClient(ep.AddressFamily);
			_timeout = timeout;
		}

		/// <summary>
		/// synchronous execution of an RPC request
		/// </summary>
		/// <typeparam name="TReq"></typeparam>
		/// <typeparam name="TResp"></typeparam>
		/// <param name="callBody"></param>
		/// <param name="reqArgs"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public IDisposable Request<TReq, TResp>(call_body callBody, TReq reqArgs, Action<TResp> completed, Action<Exception> excepted)
		{
			Exception resEx = null;
			TResp respArgs = default(TResp);

			rpc_msg reqHeader = new rpc_msg()
			{
				xid = 0xF1E2D3C4,
				body = new body()
				{
					mtype = msg_type.CALL,
					cbody = callBody
				}
			};

			try
			{
				UdpDatagram dtg = new UdpDatagram();
				Writer w = Toolkit.CreateWriter(dtg);
				w.Write(reqHeader);
				w.Write(reqArgs);

				byte[] outBuff = dtg.ToArray();

				Log.Trace(() => "sending byte dump: " + outBuff.ToDisplay());

				_client.Send(outBuff, outBuff.Length, _ep);
				MessageReader mr = new MessageReader();
				Reader r = Toolkit.CreateReader(mr);
				rpc_msg respMsg;
				

				long endTime = Stopwatch.GetTimestamp() + _timeout * System.Diagnostics.Stopwatch.Frequency / 1000;
				_client.Client.ReceiveTimeout = _timeout;

				while(true)
				{
					IPEndPoint ep = _ep;
					mr.Bytes = _client.Receive(ref ep);

					respMsg = r.Read<rpc_msg>();
					if(respMsg.xid == reqHeader.xid)
						break;

					int nextTimeout = (int)((double)((endTime - Stopwatch.GetTimestamp()) * 1000) / Stopwatch.Frequency);

					if (nextTimeout <= 0)
						throw new SocketException((int)SocketError.TimedOut);
					else
						_client.Client.ReceiveTimeout = nextTimeout;
				}

				Log.Trace(() => "received byte dump: " + mr.Bytes.ToDisplay());

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
				completed(respArgs);
			else
				excepted(resEx);
			
			return null;
		}
	}
}

