using System;
using System.Collections.Generic;

namespace Xdr.Example
{
	public partial class FileType
	{
		public class WriteContext
		{
			private FileType _item;
			private Writer _writer;
			private Action _completed;
			private Action<Exception> _excepted;

			public WriteContext(Writer writer, FileType item, Action completed, Action<Exception> excepted)
			{
				_writer = writer;
				_item = item;
				_completed = completed;
				_excepted = excepted;

				EnumHelper<FileKind>.EnumToInt(_item.Type, Switch_Converted, _excepted);
			}

			private void Switch_Converted(int sw)
			{
				if (sw == 1)
				{
					_writer.Write<FileKind>(_item.Type, Switch_1_Writed, _excepted);
					return;
				}
				if (sw == 2)
				{
					_writer.Write<FileKind>(_item.Type, Switch_2_Writed, _excepted);
					return;
				}
				if (sw == 0)
				{
					_writer.Write<FileKind>(_item.Type, _completed, _excepted);
					return;
				}
				
				_excepted(new InvalidCastException(string.Format("unexpected value: `{0}'", sw)));
			}

			private void Switch_1_Writed()
			{
				_writer.WriteVar<string>(_item.Creator, CompleteFile.MaxNameLen, _completed, _excepted);
			}

			private void Switch_2_Writed()
			{
				_writer.WriteVar<string>(_item.Interpretor, CompleteFile.MaxNameLen, _completed, _excepted);
			}

			/*
			public WriteContext(Writer writer, FileType item, Action completed, Action<Exception> excepted)
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
						_writer.WriteVar<string>(_item.Creator, CompleteFile.MaxNameLen, _completed, _excepted);
						break;
	
					case FileKind.Exec:
						_writer.WriteVar<string>(_item.Interpretor, CompleteFile.MaxNameLen, _completed, _excepted);
						break;
	
					case FileKind.Text:
						_completed();
						break;
	
					default:
						_excepted(new InvalidCastException(string.Format("unexpected value: `{0}'", _item.Type)));
						break;
				}
			}
			*/
		}
	}
}

