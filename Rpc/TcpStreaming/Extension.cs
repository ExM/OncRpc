using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.MessageProtocol;
using Xdr;
using System.IO;
using System.Threading;

namespace Rpc.TcpStreaming
{
	internal static class Extension
	{
		internal static void AsyncRead(this Stream stream, Action<Exception, TcpReader> completed)
		{
			//HACK: here you need to implement a timeout interrupt
			var context = new ReadContext()
			{
				Stream = stream, Completed = completed
			};
			context.BeginRead();
		}
		
		private class ReadContext
		{
			public Stream Stream;
			public TcpReader TcpReader = new TcpReader();
			public Action<Exception, TcpReader> Completed;
			
			private byte[] _buf;
			private int _pos;
			private int _leftToRead;
			private bool _lastBlock;
			
			
			public void BeginRead()
			{
				try
				{
					BeginReadRecordMark();
				}
				catch(Exception ex)
				{
					ThreadPool.QueueUserWorkItem((state) => Completed(ex, null));
				}
			}
			
			private void BeginReadRecordMark()
			{
				_buf = new byte[4];
				_pos = 0;
				_leftToRead = 4;
				
				Stream.BeginRead(_buf, _pos, _leftToRead, EndReadRecordMark, null);
			}
			
			private void EndReadRecordMark(IAsyncResult ar)
			{
				try
				{
					int read = Stream.EndRead(ar);
					
					if(read <= 0)
						throw new EndOfStreamException();
					
					_leftToRead -= read;
					_pos += read;
					
					if(_leftToRead <= 0)
					{
						ExtractRecordMark();
						Stream.BeginRead(_buf, _pos, _leftToRead, EndReadBody, null);
					}
					else
					{
						Stream.BeginRead(_buf, _pos, _leftToRead, EndReadRecordMark, null);
					}
				}
				catch(Exception ex)
				{
					Completed(ex, null);
				}
			}
			
			private void EndReadBody(IAsyncResult ar)
			{
				try
				{
					int read = Stream.EndRead(ar);
					
					if(read <= 0)
						throw new EndOfStreamException();
					
					_leftToRead -= read;
					_pos += read;
					
					if(_leftToRead <= 0)
					{
						TcpReader.AppendBlock(_buf);
						if(!_lastBlock)
						{
							BeginReadRecordMark();
							return;
						}
					}
					else
					{
						Stream.BeginRead(_buf, _pos, _leftToRead, EndReadBody, null);
						return;
					}
				}
				catch(Exception ex)
				{
					Completed(ex, null);
				}
				
				Completed(null, TcpReader);
			}
			
			private void ExtractRecordMark()
			{
				_leftToRead =
					((_buf[0] & 0x7F) << 0x18) |
					(_buf[1] << 0x10) |
					(_buf[2] << 0x08) |
					(_buf[3]);
				
				_buf = new byte[_leftToRead];
				_pos = 0;
				_lastBlock = (_buf[0] & 0x80) == 0;
			}
		}

		internal static void AsyncWrite(this Stream stream, LinkedList<byte[]> blocks, Action<Exception> completed)
		{
			//HACK: here you need to implement a timeout interrupt
			var context = new WriteContext()
			{
				Stream = stream, Blocks = blocks, Completed = completed
			};
			context.BeginWrite();
		}
		
		private class WriteContext
		{
			public Stream Stream;
			public LinkedList<byte[]> Blocks;
			public Action<Exception> Completed;
			public Exception Error = null;
			
			private void WrapCompleted(object arg)
			{
				Completed(Error);
			}
			
			public void BeginWrite()
			{
				if(Blocks.First == null)
				{
					ThreadPool.QueueUserWorkItem(WrapCompleted);
					return;
				}
			
				byte[] block = Blocks.First.Value;
				Blocks.RemoveFirst();
				
				try
				{
					Stream.BeginWrite(block, 0, block.Length, EndWrite, null);
				}
				catch(Exception ex)
				{
					Error = ex;
					ThreadPool.QueueUserWorkItem(WrapCompleted);
				}
			}
		
			private void EndWrite(IAsyncResult ar)
			{
				try
				{
					Stream.EndWrite(ar);
					
					if(Blocks.First != null)
					{
						byte[] block = Blocks.First.Value;
						Blocks.RemoveFirst();
						Stream.BeginWrite(block, 0, block.Length, EndWrite, null);
						return;
					}
				}
				catch(Exception ex)
				{
					Completed(ex);
					return;
				}
				
				Completed(null);
			}
		}
	}
}
