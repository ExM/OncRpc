using System;
using System.Threading;
using System.IO;

namespace Xdr
{
	public class AsyncStream: IByteReader, IByteWriter
	{
		private Stream _stream;
		
		public AsyncStream(Stream stream)
		{
			_stream = stream;
		}

		public void Read(uint uicount, Action<byte[]> completed, Action<Exception> excepted)
		{
			if(uicount == 0)
			{
				ThreadPool.QueueUserWorkItem((arg) => completed(new byte[0]));
				return;
			}
			
			if(uicount >= int.MaxValue)
				throw new ArgumentOutOfRangeException("uicount");
			int count = (int)uicount;
			
			byte[] buff = new byte[count];
			_stream.BeginRead(buff, 0, count,
				(ar) => OnRead(ar, buff, 0, count, completed, excepted), null);
		}
		
		private void OnRead(IAsyncResult ar, byte[] buff, int offset, int count, Action<byte[]> completed, Action<Exception> excepted)
		{
			try
			{
				int read = _stream.EndRead(ar);
				
				if(read < count)
				{
					count -= read;
					offset += read;
					_stream.BeginRead(buff, offset, count,
						(ar2) => OnRead(ar2, buff, offset, count, completed, excepted), null);
				}
			}
			catch(Exception err)
			{
				excepted(err);
				return;
			}
			
			completed(buff);
		}

		public void Write(byte[] buff, Action completed, Action<Exception> excepted)
		{
			_stream.BeginWrite(buff, 0, buff.Length, (ar) =>
			{
				try
				{
					_stream.EndWrite(ar);
				}
				catch (Exception err)
				{
					excepted(err);
					return;
				}
				completed();
			}, null);
		}
	}
}

