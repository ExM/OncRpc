using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using NLog;
using Rpc.TcpStreaming;
using Rpc.MessageProtocol;
using Xdr;

namespace Rpc.Connectors
{
	public class TcpSession: IRpcSession
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly TcpClientWrapper _client;
		private readonly int _maxBlock;

		private bool _connected = false;
		private ITicket _sendingTicket = null;

		private readonly object _sync = new object();
		private bool _receivingInProgress = false;
		private Dictionary<uint, ITicket> _handlers = new Dictionary<uint, ITicket>();

		public TcpSession(IPEndPoint ep, int blockSize = 1024 * 4)
		{
			_client = new TcpClientWrapper(ep);
			_maxBlock = blockSize;
		}

		public void AsyncSend(ITicket ticket)
		{
			if (_sendingTicket != null)
				throw new InvalidOperationException("ticket already sending");
			_sendingTicket = ticket;

			lock(_sync)
				_handlers.Add(_sendingTicket.Xid, _sendingTicket);

			if (_connected)
				ThreadPool.QueueUserWorkItem(BuildMessage);
			else
			{
				_connected = true;
				_client.AsyncConnect(OnConnected);
			}
		}

		private void OnConnected(Exception ex)
		{
			if (ex != null)
				OnException(ex);
			else
				BuildMessage(null);
		}

		private void BuildMessage(object state)
		{

			LinkedList<byte[]> blocks;
			try
			{
				TcpWriter tw = new TcpWriter(_maxBlock);
				_sendingTicket.BuildRpcMessage(tw);
				blocks = tw.Build();
			}
			catch(Exception ex)
			{
				Log.Debug("TCP message not builded (xid:{0}) reason: {1}", _sendingTicket.Xid, ex);
				lock(_sync)
					_handlers.Remove(_sendingTicket.Xid);
				_sendingTicket.Except(ex);
				OnSend();
				return;
			}

			BeginReceive();
			Log.Debug("Begin sending TCP message (xid:{0})", _sendingTicket.Xid);
			_client.AsyncWrite(blocks, OnBlocksWrited);
		}

		private void BeginReceive()
		{
			lock (_sync)
			{
				if (_receivingInProgress)
					return;

				if (_handlers.Count == 0)
				{
					Log.Debug("Receive stop. No handlers.");
					return;
				}

				_receivingInProgress = true;
			}

			Log.Trace("Wait response.");
			_client.AsyncRead(OnMessageReaded);
		}

		private void OnMessageReaded(Exception err, TcpReader tcpReader)
		{
			if (err != null)
			{
				Log.Debug("No receiving TCP messages. Reason: {0}", err);
				OnException(err);
				return;
			}

			lock (_sync)
				_receivingInProgress = false;

			rpc_msg respMsg = null;
			Reader r = null;

			try
			{
				r = Toolkit.CreateReader(tcpReader);
				respMsg = r.Read<rpc_msg>();
			}
			catch (Exception ex)
			{
				Log.Info("Parse exception: {0}", ex);
				BeginReceive();
				return;
			}

			Log.Trace("Received response xid:{0}", respMsg.xid);
			ITicket ticket = EnqueueTicket(respMsg.xid);
			BeginReceive();
			if (ticket == null)
				Log.Debug("No handler for xid:{0}", respMsg.xid);
			else
				ticket.ReadResult(tcpReader, r, respMsg);
		}

		private ITicket EnqueueTicket(uint xid)
		{
			lock (_sync)
			{
				ITicket result;
				if (!_handlers.TryGetValue(xid, out result))
					return null;

				_handlers.Remove(xid);
				return result;
			}
		}

		private void OnBlocksWrited(Exception ex)
		{
			if (ex != null)
			{
				Log.Debug("TCP message not sended (xid:{0}) reason: {1}", _sendingTicket.Xid, ex);
				OnException(ex);
			}
			else
			{
				Log.Debug("TCP message sended (xid:{0})", _sendingTicket.Xid);
				OnSend();
			}
		}

		public void RemoveTicket(ITicket ticket)
		{
			lock (_sync)
				_handlers.Remove(ticket.Xid);
		}

		public void Close(Exception ex)
		{
			Log.Debug("Close session.");
			ITicket[] tickets;

			lock (_sync)
			{
				tickets = _handlers.Values.ToArray();
				_handlers.Clear();
			}

			foreach (var t in tickets)
				t.Except(ex);

			_client.Close();
		}

		public event Action<IRpcSession, Exception> OnExcepted;

		private void OnException(Exception ex)
		{
			Action<IRpcSession, Exception> copy = OnExcepted;
			if (copy != null)
				copy(this, ex);
		}

		public event Action<IRpcSession> OnSended;

		private void OnSend()
		{
			_sendingTicket = null;
			Action<IRpcSession> copy = OnSended;
			if (copy != null)
				copy(this);
		}
	}
}
