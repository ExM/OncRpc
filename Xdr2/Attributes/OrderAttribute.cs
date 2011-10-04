using System;

namespace Xdr2
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class OrderAttribute: Attribute
	{
		public uint Order {get; private set;}

		public OrderAttribute(uint order)
		{
			Order = order;
		}
	}
}

