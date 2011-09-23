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

		private ArrayWriter(IWriter writer, T[] items, Action completed, Action<Exception> excepted)
		{
			_writer = writer;
			_items = items;
			_completed = completed;
			_excepted = excepted;
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
		
		public static void WriteFix(IWriter writer, T[] items, uint len, Action completed, Action<Exception> excepted)
		{
			if(items.LongLength == len)
			{
				var context = new ArrayWriter<T>(writer, items, completed, excepted);
				context.WriteNextItem();
			}
			else
				writer.Throw(new InvalidOperationException("unexpected length"), excepted);
		}
		
		public static void WriteVar(IWriter writer, T[] items, uint max, Action completed, Action<Exception> excepted)
		{
			if(items.LongLength <= max)
			{
				var context = new ArrayWriter<T>(writer, items, completed, excepted);
				writer.WriteUInt32((uint)items.LongLength, context.WriteNextItem, excepted);
			}
			else
				writer.Throw(new InvalidOperationException("unexpected length"), excepted);
		}
	}
}
