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

		private ArrayReader(IReader reader, uint len, bool fix, Action<T[]> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;
			
			if (fix)
			{
				_target = new T[len];
				ReadNextItem();
			}
			else
			{
				_maxlength = len;
				_reader.ReadUInt32(Lenght_Readed, _excepted);
			}
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
		
		public static void Read(IReader reader, uint len, bool fix, Action<T[]> completed, Action<Exception> excepted)
		{
			new ArrayReader<T>(reader, len, fix, completed, excepted);
		}
	}
}
