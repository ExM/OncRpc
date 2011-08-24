using System;

namespace Xdr.TestDtos
{
	public class SimplyInt_ReadContext
	{
		private SimplyInt _target = new SimplyInt();
		private IReader _reader;
		private Action<SimplyInt> _completed;
		private Action<Exception> _excepted;

		private SimplyInt_ReadContext(IReader reader, Action<SimplyInt> completed, Action<Exception> excepted)
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
		
		public static void Read(IReader reader, Action<SimplyInt> completed, Action<Exception> excepted)
		{
			new SimplyInt_ReadContext(reader, completed, excepted);
		}
	}
}

