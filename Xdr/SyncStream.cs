using System;
using System.IO;

namespace Xdr
{
	public class SyncStream: IByteReader, IByteWriter
	{
		private Stream _stream;
		
		public SyncStream(Stream stream)
		{
			_stream = stream;
		}

		public void Read(uint uicount, Action<byte[]> completed, Action<Exception> excepted)
		{
			if(uicount >= int.MaxValue)
				throw new ArgumentOutOfRangeException("uicount");
			int count = (int)uicount;
			
			byte[] result = null;
			try
			{
				result = new byte[count];
				int offset = 0;
				while(true)
				{
					int read = _stream.Read(result, offset, count);
					if (read == 0)
						throw new InvalidOperationException("empty data");
					if(read >= count)
						break;
					count -= read;
					offset += read;
				}
			}
			catch(Exception err)
			{
				excepted(err);
				return;
			}
			
			completed(result);
		}

		public void Write(byte[] buffer, Action completed, Action<Exception> excepted)
		{
			try
			{
				_stream.Write(buffer, 0, buffer.Length);
			}
			catch(Exception err)
			{
				excepted(err);
				return;
			}
			
			completed();
		}
	}
}

