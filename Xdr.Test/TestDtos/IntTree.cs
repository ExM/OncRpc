using System;

namespace Xdr.Example
{
	public class IntTree
	{
		[Field(0)]
		public int Field1;

		[Field(1)]
		public uint Field2;

		[Field(2)]
		public int Field3;

		[Field(3)]
		public uint Field4 { get; set; }

		[Field(4)]
		public int Field5 { get; set; }
	}
}

