using System;
using Rpc;
using Rpc.MessageProtocol;
using Xdr;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using NLog;
using System.Threading;
using Rpc.Connectors;
using System.Collections.Generic;

namespace Rpc
{
	/// <summary>
	/// connector on the UDP with a asynchronous query execution
	/// </summary>
	public class AsyncUdpConnector: IConnector
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private UdpClient _client;
		private IPEndPoint _ep;

		/// <summary>
		/// connector on the UDP with a asynchronous query execution
		/// </summary>
		/// <param name="ep"></param>
		public AsyncUdpConnector(IPEndPoint ep)
		{
			Log.Info("create connector from {0}", ep);
			_ep = ep;
			_client = new UdpClient(ep.AddressFamily);

		}

		/// <summary>
		/// asynchronous execution of an RPC request
		/// </summary>
		/// <typeparam name="TReq"></typeparam>
		/// <typeparam name="TResp"></typeparam>
		/// <param name="callBody"></param>
		/// <param name="reqArgs"></param>
		/// <param name="completed"></param>
		/// <param name="excepted"></param>
		public void Request<TReq, TResp>(call_body callBody, TReq reqArgs, Action<TResp> completed, Action<Exception> excepted)
		{
			UdpReceivedHandler handler = GetHandler();

			rpc_msg reqHeader = new rpc_msg()
			{
				xid = handler.Xid,
				body = new body()
				{
					mtype = msg_type.CALL,
					cbody = callBody
				}
			};

			UdpDatagram dtg = new UdpDatagram();
			Writer w = Toolkit.CreateWriter(dtg);
			w.Write(reqHeader);
			w.Write(reqArgs);

			byte[] outBuff = dtg.ToArray();

			Log.Trace(() => "sending byte dump: " + outBuff.ToDisplay());

			_client.BeginSend(outBuff, outBuff.Length, _ep, (ar) =>
			{
				_client.EndSend(ar);
				


			}, null);

			
		}

		private object _sync = new object();
		private uint _nextXid = 0;
		private Dictionary<uint, UdpReceivedHandler> _handlers = new Dictionary<uint,UdpReceivedHandler>();

		private UdpReceivedHandler GetHandler()
		{
			UdpReceivedHandler result;
			lock (_sync)
			{
				if (_nextXid == uint.MaxValue)
					throw new IndexOutOfRangeException("number of transactions ended"); //HACK: who can handle it?

				result = new UdpReceivedHandler(_nextXid);
				_handlers.Add(_nextXid, result);
				_nextXid++;
			}
			return result;
		}
	}
}

