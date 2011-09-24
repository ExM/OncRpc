using System;

namespace Xdr.TestDtos
{
	public partial class SimplyInt
	{
		public int Field1;

		public uint Field2 { get; set; }
		
		public static void Read(Reader reader, Action<SimplyInt> completed, Action<Exception> excepted)
		{
			new ReadContext(reader, completed, excepted);
		}
		
		public static void Read2(Reader reader, Action<SimplyInt> completed, Action<Exception> excepted)
		{
			new ReadContext2(reader, completed, excepted);
		}
	}
}

