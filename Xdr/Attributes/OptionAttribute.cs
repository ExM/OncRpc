using System;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class OptionAttribute: Attribute
	{
		public OptionAttribute()
		{
		}
	}
}

