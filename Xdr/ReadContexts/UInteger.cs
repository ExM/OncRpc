using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class UInteger
	{
		private Action<UInt32> _completed;

		public UInteger(IByteReader reader, Action<UInt32> completed, Action<Exception> excepted)
		{
			_completed = completed;
			reader.Read(4, target_Readed, excepted);
		}

		private void target_Readed(byte[] val)
		{
			_completed(XdrEncoding.DecodeUInt32(val));
		}


	}
}
