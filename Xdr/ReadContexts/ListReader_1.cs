using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class ListReader<T>
	{
		private List<T> _target;
		private IReader _reader;
		private uint _length = 0;
		private uint _maxlength = 0;
		private Action<List<T>> _completed;
		private Action<Exception> _excepted;

		private ListReader(IReader reader, uint len, bool fix, Action<List<T>> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;

			_target = new List<T>();
			
			if (fix)
			{
				_length = len;
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
			_length = val;
			if (_length > _maxlength)
				_excepted(new InvalidOperationException(string.Format("unexpected length {0} items", _length)));
			else
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
		
		public static void Read(IReader reader, uint len, bool fix, Action<List<T>> completed, Action<Exception> excepted)
		{
			new ListReader<T>(reader, len, fix, completed, excepted);
		}
	}
}
