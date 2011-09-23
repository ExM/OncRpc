﻿using System;
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

		private ListWriter(IWriter writer, List<T> items, Action completed, Action<Exception> excepted)
		{
			_writer = writer;
			_items = items;
			_completed = completed;
			_excepted = excepted;
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

		public static void WriteFix(IWriter writer, List<T> items, uint len, Action completed, Action<Exception> excepted)
		{
			if(items.Count == len)
			{
				var context = new ListWriter<T>(writer, items, completed, excepted);
				context.WriteNextItem();
			}
			else
				writer.Throw(new InvalidOperationException("unexpected length"), excepted);
		}
		
		public static void WriteVar(IWriter writer, List<T> items, uint max, Action completed, Action<Exception> excepted)
		{
			if(items.Count <= max)
			{
				var context = new ListWriter<T>(writer, items, completed, excepted);
				writer.WriteUInt32((uint)items.Count, context.WriteNextItem, excepted);
			}
			else
				writer.Throw(new InvalidOperationException("unexpected length"), excepted);
		}
	}
}
