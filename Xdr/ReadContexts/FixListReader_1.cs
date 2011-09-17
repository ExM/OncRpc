using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class FixListReader<T>
	{
		private List<T> _target;
		private IReader _reader;
		private uint _length = 0;
		private Action<List<T>> _completed;
		private Action<Exception> _excepted;

		private FixListReader(IReader reader, uint len, Action<List<T>> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;

			_target = new List<T>();
			
			_length = len;
			ReadNextItem();
		}

		private void ReadNextItem()
		{
			if (_length > 0)
				_reader.Read<T>(Item_Readed, _excepted);
			else
				_completed(_target);
		}

		private void Item_Readed(T val)
		{
			_target.Add(val);
			_length--;
			ReadNextItem();
		}
		
		public static void Read(IReader reader, uint len, Action<List<T>> completed, Action<Exception> excepted)
		{
			new FixListReader<T>(reader, len, completed, excepted);
		}
	}
}
