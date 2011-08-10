using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public class Integer_ReadContext
	{
		private Action<Int32> _completed;

		public Integer_ReadContext(IByteReader reader, Action<Int32> completed, Action<Exception> excepted)
		{
			_completed = completed;
			reader.Read(4, target_Readed, excepted);
		}

		private void target_Readed(byte[] val)
		{
			_completed(Decode(val));
		}

		/// <summary>
		/// Decodes the int32.
		/// http://tools.ietf.org/html/rfc4506#section-4.1
		/// </summary>
		public static int Decode(byte[] buff)
		{
			return (buff[0] << 0x18) | (buff[1] << 0x10) | (buff[2] << 0x08) | buff[3];
		}
	}
}
