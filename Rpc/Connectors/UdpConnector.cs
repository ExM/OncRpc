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

		private void NewSession()
		{
			if (_client != null)
				_client.Close();
			_client = new UdpClient();
			_client.Connect(_ep);
		}

		public void Close()
		{
			List<ITicket> tickets = new List<ITicket>();
			lock(_sync)
			{
				tickets.AddRange(_pendingRequests);
				_pendingRequests.Clear();
				tickets.AddRange(_handlers.Values);
				_handlers.Clear();
				
				NewSession();
			}
			
			Exception ex = new TaskCanceledException("close connector");
			foreach(var t in tickets)
				t.Except(ex);
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
			while (true)
			{
				ITicket ticket;
				UdpClient clientCopy;

				lock (_sync)
				{
					if (_sendingInProgress)
						return;

					if (_pendingRequests.Count == 0)
						return;

					ticket = _pendingRequests.First.Value;
					_pendingRequests.RemoveFirst();
					ticket.Xid = _nextXid++;
					_sendingInProgress = true;

					_handlers.Add(ticket.Xid, ticket);
					clientCopy = _client;
				}

				try
				{
					byte[] datagram = ticket.BuildDatagram();
					if (datagram == null)
						continue;
					Log.Trace(DumpToLog, "sending byte dump: {0}", datagram);
					clientCopy.BeginSend(datagram, datagram.Length, DatagramSended, clientCopy);
				}
				catch (Exception ex)
				{
					ThreadPool.QueueUserWorkItem(o => OnSendException(ex));
				}

				break;
			}
		}
		
		private void DatagramSended(IAsyncResult ar)
		{
			UdpClient clientCopy = ar.AsyncState as UdpClient;
			
			bool isOldClient = true;
			
			lock(_sync)
			{
				if(clientCopy == _client)
				{
					isOldClient = false;
					_sendingInProgress = false;
				}
			}
			
			if(isOldClient)
			{
				Log.Debug("close old sender");
				try
				{
					clientCopy.EndSend(ar);
				}
				catch(Exception)
				{
				}
				clientCopy.Close();
				return;
			}

			Log.Debug("datagram sended");

			try
			{
				clientCopy.EndSend(ar);
			}
			catch(Exception ex)
			{
				OnSendException(ex);
				return;
			}
			
			if(BeginReceive())
				SendNextQueuedItem();
		}
		
		private void OnSendException(Exception ex)
		{
			List<ITicket> tickets = new List<ITicket>();
			lock(_sync)
			{
				_sendingInProgress = false;
				_receivingInProgress = false;
				
				tickets.AddRange(_handlers.Values);
				_handlers.Clear();
				
				NewSession();
			}
			
			foreach(var t in tickets)
				t.Except(ex);
			
			SendNextQueuedItem();
		}

		private bool BeginReceive()
		{
			UdpClient clientCopy;
			lock(_sync)
			{
				if(_receivingInProgress)
					return true;

				if (_handlers.Count == 0)
				{
					Log.Debug("receive stop. no handlers");
					return true;
				}
				
				_receivingInProgress = true;
				clientCopy = _client;
			}
			
			try
			{
				Log.Trace("begin receive");
				clientCopy.BeginReceive(DatagramReceived, clientCopy);
				return true;
			}
			catch(Exception ex)
			{
				ThreadPool.QueueUserWorkItem(o => OnReceiveException(ex));
				return false;
			}
		}
		
		private void OnReceiveException(Exception ex)
		{
			List<ITicket> tickets = new List<ITicket>();
			lock(_sync)
			{
				_sendingInProgress = false;
				_receivingInProgress = false;
				
				tickets.AddRange(_handlers.Values);
				_handlers.Clear();
				
				NewSession();
			}
			
			foreach(var t in tickets)
				t.Except(ex);
			
			SendNextQueuedItem();
		}
		
		private void DatagramReceived(IAsyncResult ar)
		{
			UdpClient clientCopy = ar.AsyncState as UdpClient;
			bool isOldClient = true;
			
			lock(_sync)
			{
				if(clientCopy == _client)
				{
					isOldClient = false;
					_receivingInProgress = false;
				}
			}
			
			IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
			
			if(isOldClient)
			{
				Log.Debug("close old receiver");
				try
				{
					clientCopy.EndReceive(ar, ref ep);
				}
				catch(Exception)
				{
				}
				clientCopy.Close();
				return;
			}
			
			byte[] received;
			
			try
			{
				received = clientCopy.EndReceive(ar, ref ep);
				Log.Debug("received {0} bytes", received.Length);
			}
			catch(Exception ex)
			{
				OnReceiveException(ex);
				return;
			}

			rpc_msg respMsg = null;
			MessageReader mr = null;
			Reader r = null;
			bool unexpectedMessage = false;

			try
			{
				Log.Trace(DumpToLog, "received byte dump: {0}", received);
				mr = new MessageReader(received);
				r = Toolkit.CreateReader(mr);
				respMsg = r.Read<rpc_msg>();
			}
			catch (Exception ex)
			{
				Log.Info("parse exception: {0}", ex);
				unexpectedMessage = true;
			}

			if (unexpectedMessage)
			{
				BeginReceive();
				return;
			}

			ITicket ticket = EnqueueTicket(respMsg.xid);
			BeginReceive();
			if (ticket == null)
				Log.Debug("no handler for xid:{0}", respMsg.xid);
			else
				ticket.ReadResult(mr, r, respMsg);
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

