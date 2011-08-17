using System;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class FieldAttribute: Attribute
	{
		public uint Order {get; private set;}

		public FieldAttribute(uint order)
		{
			Order = order;
		}
	}
}

