using System;
using System.Collections.Generic;

namespace Xdr2.TestDtos
{
	public class ListContainer
	{
		[Order(0), Fix(3)]
		public int[] Field1;
		
		[Order(1), Var(3)]
		public List<uint> Field2;
		
		[Order(2), Var(3)]
		public string Field3;
	}
}

