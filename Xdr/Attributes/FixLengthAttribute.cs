using System;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class FixLengthAttribute: Attribute
	{
		public uint Length {get; private set;}
		
		public FixLengthAttribute(uint length)
		{
			if(length == 0)
				throw new ArgumentException("length must be greater than zero");
			Length = length;
		}
	}
}

