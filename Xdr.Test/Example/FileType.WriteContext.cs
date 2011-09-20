using System;
using System.Collections.Generic;

namespace Xdr.Example
{
	public partial class FileType
	{
		public class WriteContext
		{
			private FileType _item;
			private IWriter _writer;
			private Action _completed;
			private Action<Exception> _excepted;
	
			public WriteContext(IWriter writer, FileType item, Action completed, Action<Exception> excepted)
			{
				_writer = writer;
				_item = item;
				_completed = completed;
				_excepted = excepted;
	
				_writer.Write<FileKind>(_item.Type, Type_Writed, _excepted);
			}
	
			private void Type_Writed()
			{
				switch (_item.Type)
				{
					case FileKind.Data:
						_writer.Write<string>(_item.Creator, _completed, _excepted);
						break;
	
					case FileKind.Exec:
						_writer.Write<string>(_item.Interpretor, _completed, _excepted);
						break;
	
					case FileKind.Text:
						_completed();
						break;
	
					default:
						_excepted(new InvalidCastException(string.Format("unexpected value: `{0}'", _item.Type)));
						break;
				}
			}
		}
	}
}

