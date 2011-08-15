using System;

namespace Xdr.Example
{
	public class IntTree
	{
		[XdrField(0)]
		public int Field1 {get; set;}

		[XdrField(1)]
		public int Field2 { get; set; }
	}
}

