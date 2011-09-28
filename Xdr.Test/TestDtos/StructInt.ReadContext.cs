using System;
using System.Collections.Generic;

namespace Xdr.TestDtos
{
	public partial struct StructInt
	{
		private class ReadContext
		{
			private StructInt _target;
			private Reader _reader;
			private Action<StructInt> _completed;
			private Action<Exception> _excepted;
	
			public ReadContext(Reader reader, Action<StructInt> completed, Action<Exception> excepted)
			{
				_reader = reader;
				_completed = completed;
				_excepted = excepted;
				_target = new StructInt();
				_reader.Read<int>(Field1_Readed, _excepted);
			}
	
			private void Field1_Readed(int val)
			{
				_target.Field1 = val;
				_reader.Read<uint>(Field2_Readed, _excepted);
			}
	
			private void Field2_Readed(uint val)
			{
				_target.Field2 = val;
				_completed(_target);
			}
		}
	}
}

