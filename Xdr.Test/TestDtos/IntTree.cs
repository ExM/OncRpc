using System;

namespace Xdr.Example
{
	public class IntTree
	{
		[XdrField(Order = 0)]
		public int Field1 {get; set;}

		[XdrField(Order = 1)]
		public int Field2 { get; set; }
	}
}

