using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using NLog;
using Rpc.MessageProtocol;
using Xdr;
using Rpc.UdpDatagrams;

namespace Rpc.Connectors
{
	public class UdpSession: IRpcSession
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly UdpClientWrapper _client;

		private ITicket _sendingTicket = null;

		private readonly object _sync = new object();
		private bool _receivingInProgress = false;
		private Dictionary<uint, ITicket> _handlers = new Dictionary<uint, ITicket>();

		public UdpSession(IPEndPoint ep)
		{
			_client = new UdpClientWrapper(ep);
		}

		public void AsyncSend(ITicket ticket)
		{
			if (_sendingTicket != null)
				throw new InvalidOperationException("ticket already sending");
			_sendingTicket = ticket;

			lock(_sync)
				_handlers.Add(_sendingTicket.Xid, _sendingTicket);

			ThreadPool.QueueUserWorkItem(BuildMessage);
		}

		private void BuildMessage(object state)
		{
			byte[] datagram;
			try
			{
				UdpWriter uw = new UdpWriter();
				_sendingTicket.BuildRpcMessage(uw);
				datagram = uw.Build();
			}
			catch(Exception ex)
			{
				Log.Debug("UDP datagram not builded (xid:{0}) reason: {1}", _sendingTicket.Xid, ex);
				lock(_sync)
					_handlers.Remove(_sendingTicket.Xid);
				_sendingTicket.Except(ex);
				OnSend();
				return;
			}


			Log.Debug("Begin sending UDP datagram (xid:{0})", _sendingTicket.Xid);
			_client.AsyncWrite(datagram, OnDatagramWrited);
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

		private void OnMessageReaded(Exception err, UdpReader udpReader)
		{
			if (err != null)
			{
				Log.Debug("No receiving UDP datagrams. Reason: {0}", err);
				OnException(err);
				return;
			}

			lock (_sync)
				_receivingInProgress = false;

			rpc_msg respMsg = null;
			Reader r = null;

			try
			{
				r = Toolkit.CreateReader(udpReader);
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
				ticket.ReadResult(udpReader, r, respMsg);
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

		private void OnDatagramWrited(Exception ex)
		{
			if (ex != null)
			{
				Log.Debug("UDP datagram not sended (xid:{0}) reason: {1}", _sendingTicket.Xid, ex);
				OnException(ex);
			}
			else
			{
				Log.Debug("UDP datagram sended (xid:{0})", _sendingTicket.Xid);
				BeginReceive();
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
