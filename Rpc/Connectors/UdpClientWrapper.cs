using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;
using Rpc.UdpDatagrams;

namespace Rpc.Connectors
{
	public class UdpClientWrapper
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
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
			lock(_sync)
			{
				try
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(UdpClient).FullName);
					
					_client.BeginReceive(EndRead, null);
				}
				catch(Exception ex)
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
			Action<Exception, UdpReader> copy = _readCompleted;
			_readCompleted = null;

			lock(_sync)
			{
				try
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(UdpClient).FullName);
					
					IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
					byte[] datagram = _client.EndReceive(ar, ref ep);
					Log.Trace(Toolkit.DumpToLog, "received datagram: {0}", datagram);
					reader = new UdpReader(datagram);
				}
				catch(Exception ex)
				{
					err = ex;
				}
			}

			copy(err, reader);
		}
		
		private Action<Exception> _writeCompleted = null;
		
		public void AsyncWrite(byte[] datagram, Action<Exception> completed)
		{
			Log.Trace(Toolkit.DumpToLog, "sending datagram: {0}", datagram);
			_writeCompleted = completed;
			lock(_sync)
			{
				try
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(UdpClient).FullName);
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
			Action<Exception> copy = _writeCompleted;
			_writeCompleted = null;
			lock(_sync)
			{
				try
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(UdpClient).FullName);
					_client.EndSend(ar);
				}
				catch(Exception ex)
				{
					err = ex;
				}
			}
			
			copy(err);
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

