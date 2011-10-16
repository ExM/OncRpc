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
	public class AsyncUdpConnector: IConnector, IDisposable
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
	
				//Log.Trace(() => "sending byte dump: " + outBuff.ToDisplay());

				handler.OutBuff = outBuff;

				EqueueSend(handler);

				//lock (_sync)
				//	_sendingHandlers.Enqueue(handler);
				//_client.BeginSend(outBuff, outBuff.Length, _ep, handler.DatagramSended, null);
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

		internal void EqueueSend(IReceivedHandler handler)
		{

			lock (_sync)
			{
				_sendingHandlers.Enqueue(handler);
				if (sending)
					return;

				sending = true;
			}

			NextBeginSend();
		}

		internal void DatagramSended(IAsyncResult ar)
		{
			IReceivedHandler handler = ar.AsyncState as IReceivedHandler;

			Log.Trace("DatagramSended (xid:{0})", handler.Xid);
			try
			{
				_client.EndSend(ar);
			}
			catch (Exception ex)
			{
				handler.Except(ex);
			}

			BeginReceive();

			NextBeginSend();
		}

		internal void NextBeginSend()
		{
			IReceivedHandler handler;
			lock (_sync)
			{
				if (_sendingHandlers.Count == 0)
				{
					Log.Trace("send queue emprty");
					sending = false;
					return;
				}

				handler = _sendingHandlers.Dequeue();
			}

			try
			{
				_client.BeginSend(handler.OutBuff, handler.OutBuff.Length, _ep, DatagramSended, handler);
				return;
			}
			catch (Exception ex)
			{
				handler.Except(ex);
			}

			NextBeginSend();
		}
		
		internal void BeginReceive()
		{
			lock(_sync)
			{
				if(receiving)
					return;
				if (_handlers.Count == 0)
					return;
				receiving = true;
			}

			Log.Info("Begin receive");

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
				lock (_sync)
				{
					if (_handlers.Count == 0)
					{
						Log.Info("no handlers");
						receiving = false;
						return;
					}
				}

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

				if (handler == null)
				{
					Log.Trace("no handler for xid:{0}", respMsg.xid);
					return;
				}
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

		private bool sending = false;

		private uint _nextXid = 0;
		private Dictionary<uint, IReceivedHandler> _handlers = new Dictionary<uint, IReceivedHandler>();

		private Queue<IReceivedHandler> _sendingHandlers = new Queue<IReceivedHandler>();

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

		public void Dispose()
		{
			Log.Info("dispose");
			_client.Close();
		}
	}
}

