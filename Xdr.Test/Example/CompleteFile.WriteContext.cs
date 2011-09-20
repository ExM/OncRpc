using System;
using System.Collections.Generic;

namespace Xdr.Example
{
	public partial class CompleteFile
	{
		private class WriteContext
		{
			private CompleteFile _item;
			private IWriter _writer;
			private Action _completed;
			private Action<Exception> _excepted;
	
			public WriteContext(IWriter writer, CompleteFile item, Action completed, Action<Exception> excepted)
			{
				_writer = writer;
				_item = item;
				_completed = completed;
				_excepted = excepted;
				
				_writer.Write<string>(_item.FileName, FileName_Writed, _excepted);
			}
	
			private void FileName_Writed()
			{
				_writer.Write<FileType>(_item.Type, Type_Writed, _excepted);
			}
	
			private void Type_Writed()
			{
				_writer.Write<string>(_item.Owner, Owner_Writed, _excepted);
			}
	
			private void Owner_Writed()
			{
				_writer.WriteVar<byte[]>(_item.Data, _completed, _excepted);
			}
		}
	}
}

