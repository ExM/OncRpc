using System;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class VarAttribute: Attribute
	{
		public uint MaxLength {get; private set;}
		
		public VarAttribute()
		{
			MaxLength = uint.MaxValue;
		}
		
		public VarAttribute(uint maxLength)
		{
			if(maxLength == 0)
				throw new ArgumentException("length must be greater than zero");
			MaxLength = maxLength;
		}
	}
}

