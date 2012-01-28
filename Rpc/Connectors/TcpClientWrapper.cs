using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Rpc.TcpStreaming;
using System.Collections.Generic;
using NLog;

namespace Rpc
{
	public class TcpClientWrapper
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
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

		private Action<Exception> _connectCompleted = null;

		public void AsyncConnect(Action<Exception> completed)
		{
			try
			{
				lock(_sync)
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(TcpClient).FullName);

					if(_connected || _connectCompleted != null)
						throw new InvalidOperationException("already connecting");
				}
				
				_connectCompleted = completed;
				_client.BeginConnect(_ep.Address, _ep.Port, OnConnected, null);
			}
			catch(Exception ex)
			{
				Log.Debug("Unable to connected to {0}. Reason: {1}", _ep, ex);
				_connectCompleted = null;
				ThreadPool.QueueUserWorkItem(_ => completed(ex));
			}
		}

		private void OnConnected(IAsyncResult ar)
		{
			Action<Exception> copy = _connectCompleted;
			_connectCompleted = null;

			try
			{
				lock(_sync)
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(TcpClient).FullName);

					_client.EndConnect(ar);
					_connected = true;
					_stream = _client.GetStream();
				}
			}
			catch(Exception ex)
			{
				Log.Debug("Unable to connected to {0}. Reason: {1}", _ep, ex);
				copy(ex);
			}
			
			Log.Debug("Sucess connect to {0}", _ep);
			copy(null);
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

