using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.Contexts
{
	public class ListReader<T>
	{
		private List<T> _target;
		private Reader _reader;
		private uint _length = 0;
		private uint _maxlength = 0;
		private Action<List<T>> _completed;
		private Action<Exception> _excepted;

		private ListReader(Reader reader, Action<List<T>> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;
			_target = new List<T>();
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
		
		public static void ReadFix(Reader reader, uint len, Action<List<T>> completed, Action<Exception> excepted)
		{
			var context = new ListReader<T>(reader, completed, excepted);
			context._length = len;
			context.ReadNextItem();
		}
		
		public static void ReadVar(Reader reader, uint len, Action<List<T>> completed, Action<Exception> excepted)
		{
			var context = new ListReader<T>(reader, completed, excepted);
			context._maxlength = len;
			reader.ReadUInt32(context.Lenght_Readed, excepted);
		}
	}
}
