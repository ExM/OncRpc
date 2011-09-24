using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public class TreeNode
	{
		[Order(0)]
		public int Field1;
		
		public string Field2 {get; set;}

		public string Field3;
	}
}
