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

namespace Rpc
{
	/// <summary>
	/// connector on the UDP with a asynchronous query execution
	/// </summary>
	public class UdpConnector: IDisposable, IConnector
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly IPEndPoint _ep;
		private readonly object _sync = new object();

		private UdpClient _client;
		private uint _nextXid = 0;
		private UdpSession _session;

		private bool _sendingInProgress = false;
		private readonly Queue<ITicket> _pendingRequests = new Queue<ITicket>();
		
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

		private void NewSession()
		{
			if (_client != null)
				_client.Close();
			_client = new UdpClient();
			_client.Connect(_ep);
			_session = new UdpSession(_client, OnSendCompleted, OnReceiveStop);
		}

		public void Close()
		{
			Exception ex = new TaskCanceledException("close connector");
			UdpSession prevSession;

			List<ITicket> tickets = new List<ITicket>();
			lock (_sync)
			{
				tickets.AddRange(_pendingRequests);
				_pendingRequests.Clear();

				prevSession = _session;
				NewSession();
			}

			foreach (var t in tickets)
				t.Except(ex);

			prevSession.Close(ex);
		}
		
		public Task<TResp> Request<TReq, TResp>(call_body callBody, TReq reqArgs)
		{
			Ticket<TReq, TResp> ticket = new Ticket<TReq, TResp>()
			{
				CallBody = callBody,
				Request = reqArgs,
				TaskSrc = new TaskCompletionSource<TResp>()
			};
			
			lock(_sync)
				_pendingRequests.Enqueue(ticket);

			SendNextQueuedItem();

			return ticket.TaskSrc.Task;
		}
		
		private void SendNextQueuedItem()
		{
			ITicket ticket;
			lock(_sync)
			{
				if (_sendingInProgress)
					return;

				if (_pendingRequests.Count == 0)
					return;

				ticket = _pendingRequests.Dequeue();
				ticket.Xid = _nextXid++;
				_sendingInProgress = true;

				_session.AsyncSend(ticket);
			}
		}

		private void OnSendCompleted(UdpSession session, Exception ex, ITicket ticket)
		{
			if (ex != null)
				ticket.Except(ex);

			lock (_sync)
			{
				if (_session != session)
					return;
				
				_sendingInProgress = false;
				if (ex != null)
					NewSession();
			}

			SendNextQueuedItem();
		}

		private void OnReceiveStop(UdpSession session, Exception ex)
		{
			lock (_sync)
			{
				if (_session == session && ex != null)
					NewSession();
			}

			if(ex != null)
				session.Close(ex);
		}

		public void Dispose()
		{
			Close();
		}
	}
}

