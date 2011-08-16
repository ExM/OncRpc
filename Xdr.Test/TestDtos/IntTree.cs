using System;

namespace Xdr.Example
{
	public class IntTree
	{
		[XdrField(0)]
		public int Field1;

		[XdrField(1)]
		public uint Field2;

		[XdrField(2)]
		public int Field3;

		[XdrField(3)]
		public uint Field4;

		[XdrField(4)]
		public int Field5 { get; set; }
	}
}

