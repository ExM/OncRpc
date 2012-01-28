using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Rpc.TcpStreaming;
using System.Collections.Generic;
using Rpc.UdpDatagrams;

namespace Rpc
{
	public class UdpClientWrapper
	{
		private readonly IPEndPoint _ep;
		
		private object _sync = new object();
		private bool _disposed = false;
		private UdpClient _client;
		
		public UdpClientWrapper(IPEndPoint ep)
		{
			_ep = ep;
			_client = new UdpClient();
			_client.Connect(_ep);
		}

		private Action<Exception, UdpReader> _readCompleted = null;

		public void AsyncRead(Action<Exception, UdpReader> completed)
		{
			_readCompleted = completed;
			lock (_sync)
			{
				if (_disposed)
				{
					ThreadPool.QueueUserWorkItem((state) =>
						completed(new ObjectDisposedException(typeof(UdpClient).FullName), null));
					return;
				}


				try
				{
					_client.BeginReceive(EndRead, null);
				}
				catch (Exception ex)
				{
					_readCompleted = null;
					ThreadPool.QueueUserWorkItem((state) => completed(ex, null));
				}
			}
		}

		private void EndRead(IAsyncResult ar)
		{
			UdpReader reader = null;
			Exception err = null;
			Action<Exception, UdpReader> completedCopy = _readCompleted;
			_readCompleted = null;

			lock (_sync)
			{
				if (_disposed)
					err = new ObjectDisposedException(typeof(UdpClient).FullName);
				else
				{
					try
					{
						IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
						byte[] datagram = _client.EndReceive(ar, ref ep);
						reader = new UdpReader(datagram);
					}
					catch (Exception ex)
					{
						err = ex;
					}
				}
			}

			completedCopy(err, reader);
		}
		
		private Action<Exception> _writeCompleted = null;
		
		public void AsyncWrite(byte[] datagram, Action<Exception> completed)
		{
			_writeCompleted = completed;
			lock(_sync)
			{
				if(_disposed)
				{
					ThreadPool.QueueUserWorkItem((state) =>
						completed(new ObjectDisposedException(typeof(TcpClient).FullName)));
					return;
				}
				
				try
				{
					_client.BeginSend(datagram, datagram.Length, EndWrite, null);
				}
				catch(Exception ex)
				{
					_writeCompleted = null;
					ThreadPool.QueueUserWorkItem((state) => completed(ex));
				}
			}
		}
		
		private void EndWrite(IAsyncResult ar)
		{
			Exception err = null;
			Action<Exception> completedCopy = _writeCompleted;
			_writeCompleted = null;
			lock(_sync)
			{
				if (_disposed)
					err = new ObjectDisposedException(typeof(TcpClient).FullName);
				else
				{
					try
					{
						_client.EndSend(ar);
					}
					catch (Exception ex)
					{
						err = ex;
					}
				}
			}
			
			completedCopy(err);
		}
		
		public void Close()
		{
			lock(_sync)
			{
				if(_disposed)
					return;
				_disposed = true;
				
				_client.Close();
			}
		}
	}
}

