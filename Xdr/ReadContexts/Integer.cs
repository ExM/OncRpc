using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class Integer
	{
		private Action<Int32> _completed;

		public Integer(IByteReader reader, Action<Int32> completed, Action<Exception> excepted)
		{
			_completed = completed;
			reader.Read(4, target_Readed, excepted);
		}

		private void target_Readed(byte[] val)
		{
			_completed(XdrEncoding.DecodeInt32(val));
		}
	}
}
