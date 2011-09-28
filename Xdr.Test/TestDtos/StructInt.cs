using System;

namespace Xdr.TestDtos
{
	public struct StructInt
	{
		[Order(0)]
		public int Field1;
		
		[Order(1)]
		public uint Field2 { get; set; }
	}
}

