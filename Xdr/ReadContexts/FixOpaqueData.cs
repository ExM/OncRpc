using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class FixOpaqueData
	{
		private uint _len;
		private byte[] _target = null;
		private IByteReader _reader;
		private Action<byte[]> _completed;
		private Action<Exception> _excepted;

		public FixOpaqueData(IByteReader reader, uint len, Action<byte[]> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_len = len;
			_completed = completed;
			reader.Read(_len, Target_Readed, excepted);
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
