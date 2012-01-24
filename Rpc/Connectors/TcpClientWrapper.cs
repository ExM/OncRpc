using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Rpc.TcpStreaming;
using System.Collections.Generic;

namespace Rpc
{
	public class TcpClientWrapper
	{
		private readonly IPEndPoint _ep;
		
		private object _sync = new object();
		private bool _connected = false;
		private bool _disposed = false;
		private TcpClient _client;
		private NetworkStream _stream;
		
		
		public TcpClientWrapper(IPEndPoint ep)
		{
			_ep = ep;
			_client = new TcpClient(_ep.AddressFamily);
		}
		
		public bool Connected
		{
			get
			{
				return _connected;
			}
		}
		
		public IAsyncResult BeginConnect(AsyncCallback callback, object state)
		{
			lock(_sync)
			{
				if(_disposed)
					throw new ObjectDisposedException(typeof(TcpClient).FullName);
				
				return _client.BeginConnect(_ep.Address, _ep.Port, callback, null);
			}
		}
		
		public void EndConnect(IAsyncResult asyncResult)
		{
			lock(_sync)
			{
				if(_disposed)
					throw new ObjectDisposedException(typeof(TcpClient).FullName);
				
				_client.EndConnect(asyncResult);
				_connected = true;
				_stream = _client.GetStream();
			}
		}
		
		public void AsyncRead(Action<Exception, TcpReader> completed)
		{
			lock(_sync)
			{
				if(_disposed || !_connected)
				{
					ThreadPool.QueueUserWorkItem((state) =>
						completed(new ObjectDisposedException(typeof(TcpClient).FullName), null));
					return;
				}
				
				_stream.AsyncRead(completed);
			}
		}
		
		public void AsyncWrite(LinkedList<byte[]> blocks, Action<Exception> completed)
		{
			lock(_sync)
			{
				if(_disposed || !_connected)
				{
					ThreadPool.QueueUserWorkItem((state) =>
						completed(new ObjectDisposedException(typeof(TcpClient).FullName)));
					return;
				}
				
				_stream.AsyncWrite(blocks, completed);
			}
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

