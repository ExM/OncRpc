using System;
using System.Collections.Generic;

namespace Xdr.Example
{
	public partial class CompleteFile
	{
		private class ReadContext
		{
			private CompleteFile _target;
			private Reader _reader;
			private Action<CompleteFile> _completed;
			private Action<Exception> _excepted;
	
			public ReadContext(Reader reader, Action<CompleteFile> completed, Action<Exception> excepted)
			{
				_reader = reader;
				_completed = completed;
				_excepted = excepted;
	
				_target = new CompleteFile();
				_reader.ReadVar<string>(CompleteFile.MaxNameLen, FileName_Readed, _excepted);
			}
	
			private void FileName_Readed(string val)
			{
				_target.FileName = val;
				_reader.Read<FileType>(Type_Readed, _excepted);
			}
	
			private void Type_Readed(FileType val)
			{
				_target.Type = val;
				_reader.ReadVar<string>(CompleteFile.MaxUserName, Owner_Readed, _excepted);
			}
	
			private void Owner_Readed(string val)
			{
				_target.Owner = val;
				_reader.ReadVar<byte[]>(CompleteFile.MaxFileLen, Data_Readed, _excepted);
			}
	
			private void Data_Readed(byte[] val)
			{
				_target.Data = val;
				_completed(_target);
			}
		}
	}
}

