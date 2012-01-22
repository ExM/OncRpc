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
	/// <summary>
	/// connector on the UDP with a asynchronous query execution
	/// </summary>
	public class UdpConnector : IDisposable, IConnector, ITicketOwner
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly IPEndPoint _ep;
		private readonly object _sync = new object();

		private UdpClient _client = null;
		private uint _nextXid = 0;

		private bool _sendingInProgress = false;
		private bool _receivingInProgress = false;

		private Dictionary<uint, ITicket> _handlers = new Dictionary<uint, ITicket>();
		private LinkedList<ITicket> _pendingRequests = new LinkedList<ITicket>();
		
		/// <summary>
		/// connector on the UDP with a asynchronous query execution
		/// </summary>
		/// <param name="ep"></param>
		public UdpConnector(IPEndPoint ep)
		{
			Log.Info("create connector from {0}", ep);
			_ep = ep;

			NewSession();
		}

		private UdpClient NewSession()
		{
			UdpClient prevClient = _client;
			_client = new UdpClient();
			_client.Connect(_ep);
			return prevClient;
		}
		
		/// <summary>
		/// Close this connection and cancel all queued tasks
		/// </summary>
		public void Close()
		{
			List<ITicket> tickets = new List<ITicket>();
			UdpClient prevClient;

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
		
		/// <summary>
		/// creates the task for the control request to the RPC server
		/// </summary>
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

		void ITicketOwner.RemoveTicket(ITicket ticket)
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
			UdpClient clientCopy = null;

			lock (_sync)
			{
				if (_sendingInProgress)
				{
					Log.Debug("already sending");
					return;
				}

				if (_pendingRequests.Count == 0)
				{
					Log.Debug("not pending requests to send");
					return;
				}

				ticket = _pendingRequests.First.Value;
				_pendingRequests.RemoveFirst();
				ticket.Xid = _nextXid++;
				_sendingInProgress = true;

				_handlers.Add(ticket.Xid, ticket);
				clientCopy = _client;
			}

			var t1 = Task.Factory.StartNew<byte[]>(ticket.BuildUdpDatagram);

			t1.ContinueWith(t1L =>
			{ // exist Exception
				lock (_sync)
				{
					_sendingInProgress = false;
					_handlers.Remove(ticket.Xid);
				}
				var ex = t1L.Exception;
				Log.Debug("datagram not builded (xid:{0}) reason: {1}", ticket.Xid, ex);
				ticket.Except(ex);
				SendNextQueuedItem();
			}, TaskContinuationOptions.NotOnRanToCompletion);
			
			t1.ContinueWith(t1L =>
			{ // exist Result
				byte[] datagram = t1L.Result;
				Log.Trace(DumpToLog, "sending byte dump: {0}", datagram);
				var t2 = Task.Factory.FromAsync<byte[], int, int>(clientCopy.BeginSend,
					(ar) => clientCopy.EndSend(ar), datagram, datagram.Length, null);

				t2.ContinueWith(t2L =>
				{ // exist Exception
					Exception ex = t2L.Exception;
					Log.Debug("datagram not sended (xid:{0}) reason: {1}", ticket.Xid, ex);
					Restart(clientCopy, ex);
				}, TaskContinuationOptions.NotOnRanToCompletion);

				t2.ContinueWith(t2L =>
				{ // exist Result
					Log.Debug("datagram sended (xid:{0})", ticket.Xid);
					lock (_sync)
					{
						if (clientCopy != _client)
							return;
						_sendingInProgress = false;
					}

					SendNextQueuedItem();
					BeginReceive();
				}, TaskContinuationOptions.OnlyOnRanToCompletion);

			}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}
		
		private void BeginReceive()
		{
			Log.Trace("begin receive");
			UdpClient clientCopy;
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

		private void Restart(UdpClient clientCopy, Exception ex)
		{
			List<ITicket> tickets;
			UdpClient prevClient;

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
		
		/// <summary>
		/// creates the task for the control request to the RPC server
		/// </summary>
		public void Dispose()
		{
			Close();
		}
	}
}
