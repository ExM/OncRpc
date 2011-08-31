using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class NullableReader<T> 
		where T : struct
	{
		private IReader _reader;
		private Action<T?> _completed;
		private Action<Exception> _excepted;

		private NullableReader(IReader reader, Action<T?> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;
			_reader.ReadUInt32(Exist_Readed, _excepted);
		}

		private void Exist_Readed(uint val)
		{
			if (val == 0)
				_completed(null);
			else if(val == 1)
				_reader.Read<T>(Item_Readed, _excepted);
			else
				_excepted(new InvalidOperationException(string.Format("unexpected value {0}", val)));
		}

		private void Item_Readed(T val)
		{
			_completed(val);
		}
		
		public static void Read(IReader reader, Action<T?> completed, Action<Exception> excepted)
		{
			new NullableReader<T>(reader, completed, excepted);
		}
	}
}
