using System;
using System.Collections.Generic;

namespace Xdr.TestDtos
{
	public class SimplyIntAttr_ReadContext
	{
		private SimplyIntAttr _target = new SimplyIntAttr();
		private IReader _reader;
		private Action<SimplyIntAttr> _completed;
		private Action<Exception> _excepted;

		private SimplyIntAttr_ReadContext(IReader reader, Action<SimplyIntAttr> completed, Action<Exception> excepted)
		{
			_reader = reader;
			_completed = completed;
			_excepted = excepted;

			_reader.ReadInt32(Field1_Readed, _excepted);
		}

		private void Field1_Readed(int val)
		{
			_target.Field1 = val;
			_reader.ReadUInt32(Field2_Readed, _excepted);
		}

		private void Field2_Readed(uint val)
		{
			_target.Field2 = val;
			_completed(_target);
		}


		public static void Read(IReader reader, Action<SimplyIntAttr> completed, Action<Exception> excepted)
		{
			new SimplyIntAttr_ReadContext(reader, completed, excepted);
		}
	}
}

