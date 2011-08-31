using System;
using System.Collections.Generic;

namespace Xdr.Example
{
	public class FileType_HandReadContext
	{
		private FileType _target;
		private IReader _reader;
		private Action<FileType> _completed;
		private Action<Exception> _excepted;

		private FileType_HandReadContext(IReader reader, Action<FileType> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;

			_target = new FileType();
			_reader.Read<FileKind>(Type_Readed, _excepted);
		}

		private void Type_Readed(FileKind val)
		{
			_target.Type = val;
			switch (val)
			{
				case FileKind.Data:
					_reader.ReadString(CompleteFile.MaxNameLen, Creator_Readed, _excepted);
					break;

				case FileKind.Exec:
					_reader.ReadString(CompleteFile.MaxNameLen, Interpretor_Readed, _excepted);
					break;

				case FileKind.Text:
					_completed(_target);
					break;

				default:
					_excepted(new InvalidCastException(string.Format("unexpected value: `{0}'", val)));
					break;
			}
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

		public static void Read(IReader reader, Action<FileType> completed, Action<Exception> excepted)
		{
			new FileType_HandReadContext(reader, completed, excepted);
		}
	}
}

