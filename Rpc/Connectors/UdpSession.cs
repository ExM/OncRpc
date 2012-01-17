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
	internal class UdpSession
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		private readonly UdpClient _client;
		private readonly object _sync = new object();
		private bool _receivingInProgress = false;
		private Action<UdpSession, Exception> _onReceiveStop;
		private Action<UdpSession, Exception, ITicket> _onSendCompleted;

		private Dictionary<uint, ITicket> _handlers = new Dictionary<uint, ITicket>();

		internal UdpSession(UdpClient client, Action<UdpSession, Exception, ITicket> onSendCompleted, Action<UdpSession, Exception> onReceiveStop)
		{
			_client = client;
			_onReceiveStop = onReceiveStop;
			_onSendCompleted = onSendCompleted;
		}

		internal void Close(Exception ex)
		{
			List<ITicket> tickets;
			lock (_sync)
			{
				tickets = _handlers.Values.ToList();
				_handlers.Clear();
			}

			foreach (var t in tickets)
				t.Except(ex);
		}

		internal void AsyncSend(ITicket ticket)
		{
			lock (_sync)
				_handlers.Add(ticket.Xid, ticket);

			try
			{
				byte[] datagram = ticket.BuildDatagram();
				//Log.Trace(() => "sending byte dump: " + datagram.ToDisplay());
				_client.BeginSend(datagram, datagram.Length, DatagramSended, ticket);
			}
			catch (Exception ex)
			{
				lock (_sync)
					_handlers.Remove(ticket.Xid);
				ThreadPool.QueueUserWorkItem(o => _onSendCompleted(this, ex, ticket));
			}
		}

		internal void DatagramSended(IAsyncResult ar)
		{
			ITicket ticket = ar.AsyncState as ITicket;

			Log.Debug("DatagramSended (xid:{0})", ticket.Xid);
			try
			{
				_client.EndSend(ar);
			}
			catch (Exception ex)
			{
				lock (_sync)
					_handlers.Remove(ticket.Xid);
				_onSendCompleted(this, ex, ticket);
			}

			BeginReceive();

			_onSendCompleted(this, null, ticket);
		}

		private void BeginReceive()
		{
			bool handlerExists = false;
			lock (_sync)
			{
				if (_receivingInProgress)
					return;

				handlerExists = _handlers.Count != 0;
				if(handlerExists)
					_receivingInProgress = true;
			}

			if (!handlerExists)
			{
				Log.Debug("End receive (no handlers)");
				_onReceiveStop(this, null);
				return;
			}
			
			try
			{
				Log.Trace("Begin receive");
				_client.BeginReceive(DatagramReceived, null);
			}
			catch (Exception ex)
			{
				_onReceiveStop(this, ex);
			}
		}

		private void DatagramReceived(IAsyncResult ar)
		{
			lock (_sync)
				_receivingInProgress = false;

			byte[] received;
			IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
			try
			{
				received = _client.EndReceive(ar, ref ep);
				Log.Trace("received {0} bytes from {1}", received.Length, ep);
			}
			catch (Exception ex)
			{
				_onReceiveStop(this, ex);
				return;
			}

			BeginReceive();
			ParseMessage(received);
		}

		private void ParseMessage(byte[] received)
		{
			ITicket ticket;
			rpc_msg respMsg;
			MessageReader mr;
			Reader r;

			try
			{
				Log.Trace(() => "received byte dump: " + received.ToDisplay());
				mr = new MessageReader(received);
				r = Toolkit.CreateReader(mr);
				respMsg = r.Read<rpc_msg>();

				ticket = EnqueueTicket(respMsg.xid);

				if (ticket == null)
				{
					Log.Debug("no handler for xid:{0}", respMsg.xid);
					return;
				}
			}
			catch (Exception ex)
			{
				Log.Info("parse exception: {0}", ex);
				return;
			}

			ticket.ReadResult(mr, r, respMsg);
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
	}
}

