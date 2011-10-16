using System;
using System.Linq;
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
	public class AsyncUdpConnector//: IConnector
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
		public IRpcRequest<TResp> Request<TReq, TResp>(call_body callBody, TReq reqArgs)
		{
			RpcRequest<TResp> handler = null;
			try
			{
				handler = NewHandler<TResp>();
	
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
	
				_client.BeginSend(outBuff, outBuff.Length, _ep, handler.DatagramSended, null);
			}
			catch(Exception ex)
			{
				if(handler == null)
					handler = new RpcRequest<TResp>(null, 0);
				ThreadPool.QueueUserWorkItem((o) => handler.Except(ex));
			}

			return handler;
		}
		
		internal void EndSend(IAsyncResult ar)
		{
			_client.EndSend(ar);
		}
		
		internal bool RemoveHandler(uint xid)
		{
			lock (_sync)
				return _handlers.Remove(xid);
		}
		
		internal void BeginReceive()
		{
			lock(_sync)
			{
				if(receiving)
					return;
				receiving = true;
			}
			
			try
			{
				_client.BeginReceive(Received, null);
			}
			catch(Exception ex)
			{
				ExceptReceive(ex);
			}
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
				ExceptReceive(ex);
				return;
			}
			
			IReceivedHandler handler;
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

		private void ExceptReceive(Exception ex)
		{
			Log.Info("receiving exception: {0}", ex);
			List<IReceivedHandler> handlers;
			lock(_sync)
			{
				handlers = _handlers.Values.ToList();
				_handlers.Clear();
				receiving = false;
			}
			
			foreach(var h in handlers)
				h.Except(ex);
		}
		
		private object _sync = new object();
		private bool receiving = false;
		
		private uint _nextXid = 0;
		private Dictionary<uint, IReceivedHandler> _handlers = new Dictionary<uint, IReceivedHandler>();

		private RpcRequest<TResp> NewHandler<TResp>()
		{
			RpcRequest<TResp> result;
			lock (_sync)
			{
				if (_nextXid == uint.MaxValue)
					throw new IndexOutOfRangeException("number of transactions ended"); //HACK: who can handle it?

				result = new RpcRequest<TResp>(this, _nextXid);
				_handlers.Add(_nextXid, result);
				_nextXid++;
			}
			return result;
		}
		
		
		private IReceivedHandler GetHandler(uint xid)
		{
			lock (_sync)
			{
				IReceivedHandler result;
				if(_handlers.TryGetValue(xid, out result))
				{
					_handlers.Remove(xid);
					return result;
				}
				else
					return null;
			}
		}
	}
}

