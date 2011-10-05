using System;

namespace Xdr2.TestDtos
{
	public partial class SimplyInt
	{
		[Order(0)]
		public int Field1;
		
		[Order(1)]
		public uint Field2 { get; set; }

		public static SimplyInt Read(Reader reader)
		{
			SimplyInt result = new SimplyInt();
			result.Field1 = reader.Read<int>();
			result.Field2 = reader.Read<uint>();
			return result;
		}

		public static SimplyInt Read2(Reader reader)
		{
			SimplyInt result = new SimplyInt();
			result.Field1 = -reader.Read<int>();
			result.Field2 = reader.Read<uint>();
			return result;
		}
	}
}

