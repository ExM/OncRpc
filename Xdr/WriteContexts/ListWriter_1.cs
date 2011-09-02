using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.WriteContexts
{
	public class ListWriter<T>
	{
		private List<T> _items;
		private IWriter _writer;
		private int _index = 0;
		private Action _completed;
		private Action<Exception> _excepted;

		private ListWriter(IWriter writer, List<T> items, bool fix, Action completed, Action<Exception> excepted)
		{
			_writer = writer;
			_items = items;
			_completed = completed;
			_excepted = excepted;
			
			if(fix)
				WriteNextItem();
			else
				_writer.WriteUInt32((uint)_items.Count, WriteNextItem, _excepted);
		}

		private void WriteNextItem()
		{
			if (_index < _items.Count)
			{
				T item = _items[_index];
				_index++;
				_writer.Write(item, WriteNextItem, _excepted);
			}
			else
				_completed();

		}

		public static void Write(IWriter writer, List<T> items, bool fix, Action completed, Action<Exception> excepted)
		{
			new ListWriter<T>(writer, items, fix, completed, excepted);
		}
	}
}
