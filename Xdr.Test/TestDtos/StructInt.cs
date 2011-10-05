using System;

namespace Xdr.TestDtos
{
	public partial struct StructInt
	{
		[Order(0)]
		public int Field1;
		
		[Order(1)]
		public uint Field2 { get; set; }
	}
}

