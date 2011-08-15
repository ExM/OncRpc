using System;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class XdrFieldAttribute: Attribute
	{
		public uint Order {get; private set;}

		public XdrFieldAttribute(uint order)
		{
			Order = order;
		}
	}
}

