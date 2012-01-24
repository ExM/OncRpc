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
using Rpc.TcpStreaming;

namespace Rpc.Connectors
{
	/// <summary>
	/// connector on the TCP with a asynchronous query execution
	/// </summary>
	public class TcpConnector : IDisposable, IConnector, ITicketOwner
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly IPEndPoint _ep;
		private readonly int _maxBlock;
		private readonly object _sync = new object();

		private TcpClient _client = null;
		private uint _nextXid = 0;

		private bool _sendingInProgress = false;
		private bool _receivingInProgress = false;

		private Dictionary<uint, ITicket> _handlers = new Dictionary<uint, ITicket>();
		private LinkedList<ITicket> _pendingRequests = new LinkedList<ITicket>();
		
		/// <summary>
		/// connector on the TCP with a asynchronous query execution
		/// </summary>
		/// <param name="ep">end point of the RPC-server</param>
		/// <param name="blockSize">maximum size of the message block (default: 4kb)</param>
		public TcpConnector(IPEndPoint ep, int blockSize = 1024 * 4)
		{
			Log.Info("create connector from {0}", ep);
			_ep = ep;
			_maxBlock = blockSize;

			NewSession();
		}

		private TcpClient NewSession()
		{
			TcpClient prevClient = _client;
			_client = new TcpClient(_ep.AddressFamily);
			return prevClient;
		}

		/// <summary>
		/// Close this connection and cancel all queued tasks
		/// </summary>
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
			TcpClient clientCopy = null;
			NetworkStream streamCopy = null;

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
					streamCopy = clientCopy.GetStream();
				
				ticket = _pendingRequests.First.Value;
				_pendingRequests.RemoveFirst();
				ticket.Xid = _nextXid++;
				_handlers.Add(ticket.Xid, ticket);
			}
			
			Task<LinkedList<byte[]>> buildTask;
			
			if(streamCopy == null)
			{//no connection
				var connTask = Task.Factory
					.FromAsync<IPAddress, int>(clientCopy.BeginConnect, clientCopy.EndConnect, _ep.Address, _ep.Port, null);
				
				connTask.ContinueWith(connTaskL =>
				{ // exist Exception
					var ex = connTaskL.Exception; // TODO: may be NRE (must by DisposedException)
					Log.Debug("unable to connected to {0}. Reason: {1}", _ep, ex);
					Restart(clientCopy, ex);
				}, TaskContinuationOptions.NotOnRanToCompletion);
				
				buildTask = connTask.ContinueWith(connTaskL =>
				{ // exist Result
					Log.Debug("sucess connect to {0}", _ep);
					streamCopy = clientCopy.GetStream();
					BeginReceive();
					return ticket.BuildTcpMessage(_maxBlock);
				}, TaskContinuationOptions.OnlyOnRanToCompletion);
			}
			else
				buildTask = Task.Factory.StartNew<LinkedList<byte[]>>(() => ticket.BuildTcpMessage(_maxBlock));

			buildTask.ContinueWith(btL =>
			{ // exist Exception
				var ex = btL.Exception;
				lock(_sync)
				{
					_sendingInProgress = false;
					_handlers.Remove(ticket.Xid);
				}
				Log.Debug("TCP message not builded (xid:{0}) reason: {1}", ticket.Xid, ex);
				ticket.Except(ex);
				SendNextQueuedItem();
			}, TaskContinuationOptions.OnlyOnFaulted);
			
			buildTask.ContinueWith(btL =>
			{ // exist Result
				Log.Debug("Begin sending TCP message (xid:{0})", ticket.Xid);
				streamCopy.AsyncWrite(btL.Result, (ex) =>
				{
					if(ex != null)
					{
						Log.Debug("TCP message not sended (xid:{0}) reason: {1}", ticket.Xid, ex);
						Restart(clientCopy, ex);
						return;
					}
					
					lock(_sync)
					{
						if(clientCopy != _client)
							return;
						_sendingInProgress = false;
					}

					Log.Trace("TCP message sended (xid:{0})", ticket.Xid);

					SendNextQueuedItem();
				});
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}
		
		private void BeginReceive()
		{
			Log.Trace("Begin receive.");
			TcpClient clientCopy;
			NetworkStream streamCopy = null;
			lock(_sync)
			{
				if(_receivingInProgress)
				{
					Log.Debug("Already receiving.");
					return;
				}

				if(_handlers.Count == 0)
				{
					Log.Debug("Receive stop. no handlers.");
					return;
				}
				
				clientCopy = _client;
				if (_client.Connected)
				{
					_receivingInProgress = true;
					streamCopy = clientCopy.GetStream();
				}
			}
			
			if(streamCopy == null)
				return;
			
			streamCopy.AsyncRead((err, tcpReader) =>
			{
				if(err != null)
				{
					Log.Debug("no receiving TCP messages. Reason: {0}", err);
					Restart(clientCopy, err);
					return;
				}

				lock(_sync)
				{
					if(clientCopy != _client)
						return;

					_receivingInProgress = false;
				}

				rpc_msg respMsg = null;
				Reader r = null;

				try
				{
					r = Toolkit.CreateReader(tcpReader);
					respMsg = r.Read<rpc_msg>();
				}
				catch(Exception ex)
				{
					Log.Info("parse exception: {0}", ex);
					BeginReceive();
					return;
				}
				
				Log.Trace("received response xid:{0}", respMsg.xid);
				ITicket ticket = EnqueueTicket(respMsg.xid);
				BeginReceive();
				if(ticket == null)
					Log.Debug("no handler for xid:{0}", respMsg.xid);
				else
					ticket.ReadResult(tcpReader, r, respMsg);
			});
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

		/// <summary>
		/// dispose this connection
		/// </summary>
		public void Dispose()
		{
			Close();
		}
	}
}
