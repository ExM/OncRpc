using System;

namespace Xdr.Example
{
	public class StringContainer
	{
		[Field(0)]
		public int FI0;

		[Field(1), Var(32)]
		public string FStr1;
		
		[Field(2), Var(32)]
		public string FStr2 { get; set; }

		[Field(3)]
		public int FI3;
	}
}

