using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class StringData
	{
		private uint _maxLen;
		private uint _len = 0;
		private IByteReader _reader;
		private string _target = null;
		private Action<string> _completed;
		private Action<Exception> _excepted;

		public StringData(IByteReader reader, uint maxLen, Action<string> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_maxLen = maxLen;
			_completed = completed;
			_excepted = excepted;
			_reader.Read(4, Length_Readed, _excepted);
		}

		private void Length_Readed(byte[] val)
		{
			_len = XdrEncoding.DecodeUInt32(val);
			
			if(_len == 0)
				_completed(string.Empty);
			else if (_len <= _maxLen)
				_reader.Read(_len, Target_Readed, _excepted);
			else
				_excepted(new InvalidOperationException("unexpected length"));
		}
		
		private void Target_Readed(byte[] val)
		{
			_target = Encoding.ASCII.GetString(val);
			if(_len % 4 == 0)
				_completed(_target);
			else
				_reader.Read((uint)(4 - _len % 4), Tail_Readed, _excepted);
		}
		
		private void Tail_Readed(byte[] val)
		{
			_completed(_target);
		}
	}
}
