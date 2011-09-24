using System;

namespace Xdr.TestDtos
{
	public class AllVariantOrder
	{
		[Order(0)]
		public int Field1;
		
		[Order(1)]
		public uint? Field2;
		
		[Order(2), Option]
		public AllVariantOrder Field3;
	}
}

