using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public class UnsignedInteger_ReadContext
	{
		private Action<UInt32> _completed;

		public UnsignedInteger_ReadContext(IByteReader reader, Action<UInt32> completed, Action<Exception> excepted)
		{
			_completed = completed;
			reader.Read(4, target_Readed, excepted);
		}

		private void target_Readed(byte[] val)
		{
			_completed(Decode(val));
		}

		/// <summary>
		/// Decodes the uint32.
		/// http://tools.ietf.org/html/rfc4506#section-4.2
		/// </summary>
		public static uint Decode(byte[] buff)
		{
			return (uint)((buff[0] << 0x18) | (buff[1] << 0x10) | (buff[2] << 0x08) | buff[3]);
		}
	}
}
