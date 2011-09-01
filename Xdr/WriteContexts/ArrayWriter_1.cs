using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.WriteContexts
{
	public class ArrayWriter<T>
	{
		private T[] _items;
		private IWriter _writer;
		private uint _index = 0;
		private Action _completed;
		private Action<Exception> _excepted;

		private ArrayWriter(IWriter writer, T[] items, bool fix, Action completed, Action<Exception> excepted)
		{
			_writer = writer;
			_items = items;
			_completed = completed;
			_excepted = excepted;
			
			if(fix)
			{
				
			}
			else
			{
				_writer.WriteUInt32((uint)_items.LongLength, WriteNextItem, _excepted);
			}
		}

		private void WriteNextItem()
		{
			if (_index < _items.LongLength)
			{
				T item = _items[_index];
				_index++;
				_writer.Write(item, WriteNextItem, _excepted);
			}
			else
				_completed();

		}
		
		public static void Write(IWriter writer, T[] items, bool fix, Action completed, Action<Exception> excepted)
		{
			new ArrayWriter<T>(writer, items, fix, completed, excepted);
		}
	}
}
