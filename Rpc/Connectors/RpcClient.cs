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

	public class RpcClient : IDisposable, IConnector, ITicketOwner
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly Func<IRpcSession> _sessionCreater;
		private readonly object _sync = new object();

		private IRpcSession _session = null;
		private uint _nextXid = 0;
		private bool _sendingInProgress = false;
		private LinkedList<ITicket> _pendingRequests = new LinkedList<ITicket>();


		public RpcClient(Func<IRpcSession> sessionCreater)
		{
			_sessionCreater = sessionCreater;
			NewSession();
		}

		private IRpcSession NewSession()
		{
			IRpcSession prevSession = _session;
			_session = _sessionCreater();
			_session.OnExcepted += OnSessionExcepted;
			_session.OnSended += OnSessionMessageSended;
			return prevSession;
		}

		public void RemoveTicket(ITicket ticket)
		{
			IRpcSession sessionCopy;
			lock (_sync)
			{
				if (_pendingRequests.Remove(ticket))
					return;
				sessionCopy = _session;
			}

			sessionCopy.RemoveTicket(ticket);
		}

		/// <summary>
		/// Close this connection and cancel all queued tasks
		/// </summary>
		public void Close()
		{
			Log.Debug("Close connector.");
			ITicket[] tickets;
			IRpcSession prevSession;

			lock(_sync)
			{
				tickets = _pendingRequests.ToArray();
				_pendingRequests.Clear();
				prevSession = NewSession();
			}

			var ex = new TaskCanceledException("close connector");
			foreach(var t in tickets)
				t.Except(ex);

			prevSession.Close(ex);
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
		
		private void SendNextQueuedItem()
		{
			Log.Trace("Send next queued item.");
			ITicket ticket = null;
			IRpcSession sessionCopy = null;

			lock(_sync)
			{
				if(_sendingInProgress)
				{
					Log.Debug("Already sending.");
					return;
				}

				if(_pendingRequests.Count == 0)
				{
					Log.Debug("Not pending requests to send.");
					return;
				}

				sessionCopy = _session;
				_sendingInProgress = true;

				ticket = _pendingRequests.First.Value;
				_pendingRequests.RemoveFirst();
				ticket.Xid = _nextXid++;
			}

			sessionCopy.AsyncSend(ticket);
		}

		private void OnSessionExcepted(IRpcSession session, Exception ex)
		{
			IRpcSession prevSession;

			lock (_sync)
			{
				if (session != _session)
					return;

				_sendingInProgress = false;

				prevSession = NewSession();
			}

			Log.Debug("Session excepted: {0}", ex);

			SendNextQueuedItem();
			prevSession.Close(ex);
		}

		private void OnSessionMessageSended(IRpcSession session)
		{
			lock (_sync)
			{
				if (session != _session)
					return;
				_sendingInProgress = false;
			}

			SendNextQueuedItem();
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
