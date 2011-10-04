using System;

namespace Xdr2
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class FixAttribute: Attribute
	{
		public uint Length {get; private set;}
		
		public FixAttribute(uint length)
		{
			if(length == 0)
				throw new ArgumentException("length must be greater than zero");
			Length = length;
		}
	}
}

