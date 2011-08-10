using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class VarOpaqueData
	{
		private uint _maxLen;
		private uint _len = 0;
		private byte[] _target = null;
		private IByteReader _reader;
		private Action<byte[]> _completed;
		private Action<Exception> _excepted;

		public VarOpaqueData(IByteReader reader, uint maxLen, Action<byte[]> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_maxLen = maxLen;
			_completed = completed;
			_reader.Read(4, Length_Readed, excepted);
		}

		private void Length_Readed(byte[] val)
		{
			_len = XdrEncoding.DecodeUInt32(val);
		
			if (_len > _maxLen)
				_excepted(new InvalidOperationException("unexpected length"));
			else
				_reader.Read(_len, Target_Readed, _excepted);
		}
		
		private void Target_Readed(byte[] val)
		{
			_target = val;
			
			if(_len % 4 == 0)
				_completed(_target);
			else
				_reader.Read((uint)(_len % 4), Tail_Readed, _excepted);
		}
		
		private void Tail_Readed(byte[] val)
		{
			_completed(_target);
		}
	}
}
