using System;
using System.Collections.Generic;

namespace Xdr.Example
{
	public partial class FileType
	{
		public class ReadContext
		{
			private FileType _target;
			private Reader _reader;
			private Action<FileType> _completed;
			private Action<Exception> _excepted;
	
			public ReadContext(Reader reader, Action<FileType> completed, Action<Exception> excepted)
			{
				_reader = reader;
				_completed = completed;
				_excepted = excepted;
	
				_target = new FileType();
				_reader.Read<FileKind>(Type_Readed, _excepted);
			}
			
			/*
			private void Type_Readed(FileKind val)
			{
				_target.Type = val;
				switch (val)
				{
					case FileKind.Data:
						_reader.ReadVar<string>(CompleteFile.MaxNameLen, Creator_Readed, _excepted);
						break;
	
					case FileKind.Exec:
						_reader.ReadVar<string>(CompleteFile.MaxNameLen, Interpretor_Readed, _excepted);
						break;
	
					case FileKind.Text:
						_completed(_target);
						break;
	
					default:
						_excepted(new InvalidCastException(string.Format("unexpected value: `{0}'", val)));
						break;
				}
			}
			*/
			
			private void Type_Readed(FileKind val)
			{
				_target.Type = val;
				EnumHelper<FileKind>.EnumToInt(val, Switch_Converted, _excepted);
			}
			
			private void Switch_Converted(int sw)
			{
				if(sw == 1)
				{
					_reader.ReadVar<string>(CompleteFile.MaxNameLen, Creator_Readed, _excepted);
					return;
				}
				if(sw == 2)
				{
					_reader.ReadVar<string>(CompleteFile.MaxNameLen, Interpretor_Readed, _excepted);
					return;
				}
				if(sw == 0)
				{
					_completed(_target);
					return;
				}
				
				_excepted(new InvalidCastException(string.Format("unexpected value: `{0}'", sw)));
			}
	
			private void Creator_Readed(string val)
			{
				_target.Creator = val;
				_completed(_target);
			}
	
			private void Interpretor_Readed(string val)
			{
				_target.Interpretor = val;
				_completed(_target);
			}
		}
	}
}

