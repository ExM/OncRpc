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
			_client = new UdpClient(_ep);
			_client.Connect(_ep);
		}
		
		public void AsyncRead(Action<Exception, UdpReader> completed)
		{
			//TODO:
		}
		
		private Action<Exception> _writeCompleted = null;
		
		public void AsyncWrite(byte[] datagram, Action<Exception> completed)
		{
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
					_writeCompleted = completed;
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
			Action<Exception> completedCopy = null;
			lock(_sync)
			{
				completedCopy = _writeCompleted;
				_writeCompleted = null;
				if(_disposed)
					err = new ObjectDisposedException(typeof(TcpClient).FullName);
				
				try
				{
					_client.EndSend(ar);
				}
				catch(Exception ex)
				{
					err = ex;
				}
			}
			
			if(completedCopy != null)
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

