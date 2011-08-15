using System;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class VarLengthAttribute: Attribute
	{
		public uint MaxLength {get; private set;}
		
		public VarLengthAttribute(uint maxLength)
		{
			if(maxLength == 0)
				throw new ArgumentException("length must be greater than zero");
			MaxLength = maxLength;
		}
	}
}

