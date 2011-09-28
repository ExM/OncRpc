using System;
using System.Collections.Generic;

namespace Xdr.TestDtos
{
	public partial struct StructInt
	{
		private class WriteContext
		{
			private StructInt _item;
			private Writer _writer;
			private Action _completed;
			private Action<Exception> _excepted;
	
			public WriteContext(Writer writer, StructInt item, Action completed, Action<Exception> excepted)
			{
				_writer = writer;
				_item = item;
				_completed = completed;
				_excepted = excepted;
				
				_writer.Write<int>(_item.Field1, Field1_Writed, _excepted);
			}
	
			private void Field1_Writed()
			{
				_writer.Write<uint>(_item.Field2, _completed, _excepted);
			}
		}
	}
}

