using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;
using Rpc.TcpStreaming;

namespace Rpc.Connectors
{
	internal class TcpClientWrapper
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		private readonly IPEndPoint _ep;
		private bool _connected = false;
		
		private object _sync = new object();
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
					_connected = true;
					_connectCompleted = completed;
					_client.BeginConnect(_ep.Address, _ep.Port, OnConnected, null);
				}
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
		
#region async read
		
		public void AsyncRead(Action<Exception, TcpReader> completed)
		{
			//HACK: here you need to implement a timeout interrupt
			try
			{
				if(_readCompleted != null)
					throw new InvalidOperationException("already reading");
					
				_tcpReader = new TcpReader();
				_readCompleted = completed;
				
				BeginReadRecordMark();
			}
			catch(Exception ex)
			{
				_tcpReader = null;
				_readCompleted = null;
					
				ThreadPool.QueueUserWorkItem((state) => completed(ex, null));
			}
		}
		
		private TcpReader _tcpReader;
		private Action<Exception, TcpReader> _readCompleted;
		private byte[] _readBuf;
		private int _readPos;
		private int _leftToRead;
		private bool _lastReadBlock;
		
		private void BeginReadRecordMark()
		{
			_readBuf = new byte[4];
			_readPos = 0;
			_leftToRead = 4;
			
			SafeBeginRead(EndReadRecordMark);
		}
		
		private void EndReadRecordMark(IAsyncResult ar)
		{
			try
			{
				int read;
				lock(_sync)
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(TcpClient).FullName);
					read = _stream.EndRead(ar);
				}
				Log.Debug("read {0} bytes of Record Mark", read);

				if(read <= 0)
					throw new EndOfStreamException();
					
				_leftToRead -= read;
				_readPos += read;
					
				if(_leftToRead <= 0)
				{
					ExtractRecordMark();
					SafeBeginRead(EndReadBody);
				}
				else
					SafeBeginRead(EndReadRecordMark);
			}
			catch(Exception ex)
			{
				_readCompleted(ex, null);
				_tcpReader = null;
				_readCompleted = null;
				_readBuf = null;
			}
		}
		
		private void SafeBeginRead(AsyncCallback callback)
		{
			lock(_sync)
			{
				if(_disposed)
					throw new ObjectDisposedException(typeof(TcpClient).FullName);
				_stream.BeginRead(_readBuf, _readPos, _leftToRead, callback, null);
			}
		}
			
		private void EndReadBody(IAsyncResult ar)
		{
			Exception error = null;
			try
			{
				int read;
				lock(_sync)
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(TcpClient).FullName);
					read = _stream.EndRead(ar);
				}
				Log.Debug("read {0} bytes of body", read);

				if(read <= 0)
					throw new EndOfStreamException();
					
				_leftToRead -= read;
				_readPos += read;
					
				if(_leftToRead <= 0)
				{
					Log.Trace(Toolkit.DumpToLog, "received byte dump: {0}", _readBuf);
					_tcpReader.AppendBlock(_readBuf);
					if(!_lastReadBlock)
					{
						Log.Debug("body readed", read);
						BeginReadRecordMark();
						return;
					}
				}
				else
				{
					SafeBeginRead(EndReadBody);
					return;
				}
			}
			catch(Exception ex)
			{
				error = ex;
			}
			
			if(error != null)
				_tcpReader = null;
			
			_readCompleted(error, _tcpReader);
			_tcpReader = null;
			_readCompleted = null;
			_readBuf = null;
		}
		
		private void ExtractRecordMark()
		{
			_leftToRead =
					((_readBuf[0] & 0x7F) << 0x18) |
					(_readBuf[1] << 0x10) |
					(_readBuf[2] << 0x08) |
					(_readBuf[3]);
				
			_readBuf = new byte[_leftToRead];
			_readPos = 0;
			_lastReadBlock = (_readBuf[0] & 0x80) == 0;

			Log.Debug("read Record Mark lenght: {0} is last: {1}", _leftToRead, _lastReadBlock);
		}
		
#endregion
		
#region async write
		
		public void AsyncWrite(LinkedList<byte[]> blocks, Action<Exception> completed)
		{
			//HACK: here you need to implement a timeout interrupt
			try
			{
				if(_writeCompleted != null)
					throw new InvalidOperationException("already writing");
					
				_writeBlocks = blocks;
				_writeCompleted = completed;
					
				if(_writeBlocks.First == null)
					throw new ArgumentException("blocks is empty");
					
				byte[] block = _writeBlocks.First.Value;
				_byteSending = block.Length;
				_writeBlocks.RemoveFirst();
				Log.Trace(Toolkit.DumpToLog, "sending byte dump: {0}", block);
				SafeBeginWrite(block);
			}
			catch(Exception ex)
			{
				_writeCompleted = null;
				_writeBlocks = null;
				
				ThreadPool.QueueUserWorkItem((state) => completed(ex));
			}
		}
		
		private LinkedList<byte[]> _writeBlocks;
		private Action<Exception> _writeCompleted;
		private int _byteSending = 0;
		
		private void EndWrite(IAsyncResult ar)
		{
			Exception error = null;
			try
			{
				lock(_sync)
				{
					if(_disposed)
						throw new ObjectDisposedException(typeof(TcpClient).FullName);
					_stream.EndWrite(ar);
				}
				
				Log.Debug("sended {0} bytes", _byteSending);

				if(_writeBlocks.First != null)
				{
					byte[] block = _writeBlocks.First.Value;
					_byteSending = block.Length;
					_writeBlocks.RemoveFirst();
					SafeBeginWrite(block);
					return;
				}
			}
			catch(Exception ex)
			{
				error = ex;
			}
			
			_writeBlocks = null;
			_writeCompleted(error);
			_writeCompleted = null;
		}
		
		private void SafeBeginWrite(byte[] block)
		{
			lock(_sync)
			{
				if(_disposed)
					throw new ObjectDisposedException(typeof(TcpClient).FullName);
				_stream.BeginWrite(block, 0, block.Length, EndWrite, null);
			}
		}
		
#endregion
		
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

