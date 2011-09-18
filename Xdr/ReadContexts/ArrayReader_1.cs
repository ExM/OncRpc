using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class ArrayReader<T>
	{
		private T[] _target = null;
		private IReader _reader;
		private uint _index = 0;
		private uint _maxlength = 0;
		private Action<T[]> _completed;
		private Action<Exception> _excepted;

		private ArrayReader(IReader reader, Action<T[]> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;
		}
		
		private void Lenght_Readed(uint val)
		{
			if (val > _maxlength)
				_excepted(new InvalidOperationException(string.Format("unexpected length {0} items", val)));
			else
			{
				_target = new T[val];
				ReadNextItem();
			}
		}

		private void ReadNextItem()
		{
			if (_index < _target.LongLength)
				_reader.Read<T>(Item_Readed, _excepted);
			else
				_completed(_target);
		}

		private void Item_Readed(T val)
		{
			_target[_index] = val;
			_index++;
			ReadNextItem();
		}
		
		public static void ReadFix(IReader reader, uint len, Action<T[]> completed, Action<Exception> excepted)
		{
			var context = new ArrayReader<T>(reader, completed, excepted);
			context._target = new T[len];
			context.ReadNextItem();
		}
		
		public static void ReadVar(IReader reader, uint len, Action<T[]> completed, Action<Exception> excepted)
		{
			var context = new ArrayReader<T>(reader, completed, excepted);
			context._maxlength = len;
			reader.ReadUInt32(context.Lenght_Readed, excepted);
		}
	}
}
