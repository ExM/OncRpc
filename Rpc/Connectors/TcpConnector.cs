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
using System.Threading.Tasks;

namespace Rpc.Connectors
{
	public class TcpConnector : IDisposable, IConnector, ITicketOwner
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly IPEndPoint _ep;
		private readonly int _maxBlock = 1024 * 4; //4kb
		private readonly object _sync = new object();

		private TcpClient _client = null;
		private NetworkStream _stream = null;
		private uint _nextXid = 0;

		private bool _sendingInProgress = false;
		private bool _receivingInProgress = false;

		private Dictionary<uint, ITicket> _handlers = new Dictionary<uint, ITicket>();
		private LinkedList<ITicket> _pendingRequests = new LinkedList<ITicket>();
		private Queue<byte[]> _sendingTcpMessage = null;
		
		/// <summary>
		/// connector on the UDP with a asynchronous query execution
		/// </summary>
		/// <param name="ep"></param>
		public TcpConnector(IPEndPoint ep)
		{
			Log.Info("create connector from {0}", ep);
			_ep = ep;

			NewSession();
		}

		private TcpClient NewSession()
		{
			TcpClient prevClient = _client;
			_client = new TcpClient(_ep.AddressFamily);
			return prevClient;
		}

		public void Close()
		{
			List<ITicket> tickets = new List<ITicket>();
			TcpClient prevClient;

			lock(_sync)
			{
				tickets.AddRange(_pendingRequests);
				_pendingRequests.Clear();
				tickets.AddRange(_handlers.Values);
				_handlers.Clear();

				prevClient = NewSession();
			}
			
			Exception ex = new TaskCanceledException("close connector");
			foreach(var t in tickets)
				t.Except(ex);

			prevClient.Close();
		}

		public Task<TResp> CreateTask<TReq, TResp>(call_body callBody, TReq reqArgs, TaskCreationOptions options, CancellationToken token)
		{
			Ticket<TReq, TResp> ticket = new Ticket<TReq, TResp>(this, callBody, reqArgs, options, token);

			lock (_sync)
			{
				if(token.IsCancellationRequested)
					return ticket.Task;

				_pendingRequests.AddLast(ticket);
			}

			SendNextQueuedItem();
			return ticket.Task;
		}

		internal void RemoveTicket(ITicket ticket)
		{
			lock(_sync)
			{
				if(_pendingRequests.Remove(ticket))
					return;

				ITicket candidate;
				if(!_handlers.TryGetValue(ticket.Xid, out candidate))
					return;

				if(candidate != ticket)
					return;

				_handlers.Remove(ticket.Xid);
			}
		}

		private void SendNextQueuedItem()
		{
			Log.Trace("send next queued item");
			ITicket ticket = null;
			TcpClient clientCopy = null;

			lock(_sync)
			{
				if(_sendingInProgress)
				{
					Log.Debug("already sending");
					return;
				}

				if(_pendingRequests.Count == 0)
				{
					Log.Debug("not pending requests to send");
					return;
				}

				clientCopy = _client;
				_sendingInProgress = true;

				if(_client.Connected)
				{
					ticket = _pendingRequests.First.Value;
					_pendingRequests.RemoveFirst();
					ticket.Xid = _nextXid++;
					_handlers.Add(ticket.Xid, ticket);
				}
				
			}

			if(ticket == null)
			{
				Task.Factory
					.FromAsync<IPAddress, int>(clientCopy.BeginConnect, clientCopy.EndConnect, _ep.Address, _ep.Port, null)
					.ContinueWith(tL =>
				{
					var ex = tL.Exception;
					if(ex != null)
					{
						Log.Debug("unable to connected to {0}. Reason: {1}", _ep, ex);
						Restart(clientCopy, ex);
					}
					else
					{
						Log.Debug("sucess connect to {0}", _ep);
						clientCopy.GetStream(); // warming up
						SendNextQueuedItem();
					}
				});
				return;
			}

			var t1 = Task.Factory.StartNew<Queue<byte[]>>(() => ticket.BuildTcpMessage(_maxBlock));

			t1.ContinueWith(t1L =>
			{ // exist Exception
				var ex = t1L.Exception;
				lock(_sync)
				{
					_sendingInProgress = false;
					_handlers.Remove(ticket.Xid);
				}
				Log.Debug("TCP message not builded (xid:{0}) reason: {1}", ticket.Xid, ex);
				ticket.Except(ex);
				SendNextQueuedItem();
			}, TaskContinuationOptions.NotOnRanToCompletion);
			
			t1.ContinueWith(t1L =>
			{ // exist Result
				Log.Debug("Begin sending TCP message (xid:{0})", ticket.Xid);
				clientCopy.GetStream().AsyncWrite(t1L.Result, (ex) =>
				{
					if(ex != null)
					{
						Log.Debug("tcp message not sended (xid:{0}) reason: {1}", ticket.Xid, ex);
						Restart(clientCopy, ex);
						return;
					}
					
					lock(_sync)
					{
						if(clientCopy != _client)
							return;
						_sendingInProgress = false;
					}

					SendNextQueuedItem();
				});
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}
		
		private void BeginReceive()
		{
			Log.Trace("begin receive");
			TcpClient clientCopy;
			lock(_sync)
			{
				if (_receivingInProgress)
				{
					Log.Debug("already receiving");
					return;
				}

				if (_handlers.Count == 0)
				{
					Log.Debug("receive stop. no handlers");
					return;
				}
				
				_receivingInProgress = true;
				clientCopy = _client;
			}

			var t1 = Task.Factory.FromAsync<byte[]>(clientCopy.BeginReceive, (ar) =>
				{
					IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
					return clientCopy.EndReceive(ar, ref ep);
				}, null);

			t1.ContinueWith(t1L =>
			{ // exist Exception
				Exception ex = t1L.Exception;
				Log.Debug("no receiving datagrams. Reason: {0}", ex);
				Restart(clientCopy, ex);
			}, TaskContinuationOptions.NotOnRanToCompletion);

			t1.ContinueWith(t1L =>
			{ // exist Result
				byte[] datagram = t1L.Result;
				Log.Trace(DumpToLog, "received byte dump: {0}", datagram);
				Log.Debug("received {0} bytes", datagram.Length);
				lock (_sync)
				{
					if (clientCopy != _client)
						return;

					_receivingInProgress = false;
				}

				rpc_msg respMsg = null;
				MessageReader mr = null;
				Reader r = null;

				try
				{
					mr = new MessageReader(datagram);
					r = Toolkit.CreateReader(mr);
					respMsg = r.Read<rpc_msg>();
				}
				catch (Exception ex)
				{
					Log.Info("parse exception: {0}", ex);
					BeginReceive();
					return;
				}
				
				Log.Trace("received response xid:{0}", respMsg.xid);
				ITicket ticket = EnqueueTicket(respMsg.xid);
				BeginReceive();
				if (ticket == null)
					Log.Debug("no handler for xid:{0}", respMsg.xid);
				else
					ticket.ReadResult(mr, r, respMsg);

			}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private void Restart(TcpClient clientCopy, Exception ex)
		{
			List<ITicket> tickets;
			TcpClient prevClient;

			lock (_sync)
			{
				if (clientCopy != _client)
					return;

				tickets = new List<ITicket>();

				_sendingInProgress = false;
				_receivingInProgress = false;

				tickets.AddRange(_handlers.Values);
				_handlers.Clear();

				prevClient = NewSession();
			}

			SendNextQueuedItem();

			prevClient.Close();
			foreach (var t in tickets)
				t.Except(ex);
		}

		private static string DumpToLog(string frm, byte[] buffer)
		{
			return string.Format(frm, buffer.ToDisplay());
		}

		private ITicket EnqueueTicket(uint xid)
		{
			lock(_sync)
			{
				ITicket result;
				if(!_handlers.TryGetValue(xid, out result))
					return null;

				_handlers.Remove(xid);
				return result;
			}
		}
		
		public void Dispose()
		{
			Close();
		}
	}
}
