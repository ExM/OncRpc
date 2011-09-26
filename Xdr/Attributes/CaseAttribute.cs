using System;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class CaseAttribute: Attribute
	{
		public readonly object Value;
		
		public CaseAttribute(object val)
		{
			Value = val;
		}
	}
}

