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
		public IDisposable Request<TReq, TResp>(call_body callBody, TReq reqArgs, Action<TResp> completed, Action<Exception> excepted)
		{
			IUdpReceivedHandler handler = null;
			try
			{
				handler = NewHandler<TResp>(completed, excepted);
	
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
					try
					{
						_client.EndSend(ar);
						
						BeginReceive();
					}
					catch(Exception inEx)
					{
						excepted(new RpcException("request error", inEx));
					}
				}, null);
			}
			catch(Exception ex)
			{
				ThreadPool.QueueUserWorkItem((o) =>
					excepted(new RpcException("request error", ex)));
			}

			return new UdpRequestDisposer(this, handler);
		}
		
		internal void Break(IUdpReceivedHandler handler)
		{
			if(handler == null)
				return;
			
			
		}
		
		internal void BeginReceive()
		{
			lock(_sync)
			{
				if(receiving)
					return;
				receiving = true;
			}
			
			_client.BeginReceive(Received, null);
		}
		
		private void Received(IAsyncResult ar)
		{
			byte[] received;
			IPEndPoint ep = _ep;
			try
			{
				received = _client.EndReceive(ar, ref ep);
				_client.BeginReceive(Received, null);
			}
			catch(Exception ex)
			{
				Log.Info("receiving exception: {0}", ex);
				lock(_sync)
					receiving = false;
				return;
			}
			
			IUdpReceivedHandler handler;
			rpc_msg respMsg;
			MessageReader mr;
			Reader r;
			
			try
			{
				Log.Trace(() => "received byte dump: " + received.ToDisplay());
				mr = new MessageReader(received);
				r = Toolkit.CreateReader(mr);
				respMsg = r.Read<rpc_msg>();
				
				handler = GetHandler(respMsg.xid);
				if(handler == null)
					return;
			}
			catch(Exception ex)
			{
				Log.Info("parse exception: {0}", ex);
				return;
			}
			
			handler.ReadResult(mr, r, respMsg);
		}

		private object _sync = new object();
		private bool receiving = false;
		
		private uint _nextXid = 0;
		private Dictionary<uint, IUdpReceivedHandler> _handlers = new Dictionary<uint, IUdpReceivedHandler>();

		private IUdpReceivedHandler NewHandler<TResp>(Action<TResp> completed, Action<Exception> excepted)
		{
			UdpReceivedHandler<TResp> result;
			lock (_sync)
			{
				if (_nextXid == uint.MaxValue)
					throw new IndexOutOfRangeException("number of transactions ended"); //HACK: who can handle it?

				result = new UdpReceivedHandler<TResp>(_nextXid, completed, excepted);
				_handlers.Add(_nextXid, result);
				_nextXid++;
			}
			return result;
		}
		
		private IUdpReceivedHandler GetHandler(uint xid)
		{
			lock (_sync)
			{
				IUdpReceivedHandler result;
				if(_handlers.TryGetValue(xid, out result))
					return result;
				else
					return null;
			}
		}
	}
}

