using System;

namespace Xdr.TestDtos
{
	[ReadOne(typeof(SimplyIntAttr_ReadContext), "Read")]
	[ReadMany(typeof(SimplyIntAttr_ReadListContext), "ReadArray", "ReadList")]
	public class SimplyIntAttr
	{
		public int Field1;

		public uint Field2 { get; set; }
	}
}

