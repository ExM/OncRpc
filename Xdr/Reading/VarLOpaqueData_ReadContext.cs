using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public class VarLOpaqueData_ReadContext
	{
		private uint _maxLen;
		private Action<byte[]> _completed;
		private Action<Exception> _excepted;

		public VarLOpaqueData_ReadContext(IByteReader reader, Action<byte[]> completed, Action<Exception> excepted, uint maxLen = uint.MaxValue)
		{
			_maxLen = maxLen;
			_completed = completed;
			TypeCache<UInt32>.Read(reader, Length_Readed, excepted);
		}

		private void Length_Readed(uint val)
		{
			if (val > _maxLen)
				_excepted(new InvalidOperationException());
			else
				TypeCache<UInt32>.Read(reader, Length_Readed, excepted);
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
