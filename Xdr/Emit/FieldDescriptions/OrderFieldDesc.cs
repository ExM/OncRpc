using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Xdr.Emit
{
	public class OrderFieldDesc
	{
		protected MemberInfo _mi;

		public OrderFieldDesc(MemberInfo mi)
		{
			_mi = mi;
		}
	}
}

